using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.HttpClientService;

namespace UnifyNow.Forest.Services
{
    public class TicketServices
    {
        private readonly ForestDb _forestDb;
        private readonly IMapper _mapper;
        private readonly WhatsappService _whatsappService;
        private readonly SMSServices _smsService;

        private readonly IWebHostEnvironment _env;

        public TicketServices(IDbContextFactory<ForestDb> dbContextFactory, IMapper mapper, WhatsappService whatsappService, SMSServices smsService, IWebHostEnvironment env)
        {
            _forestDb = dbContextFactory.CreateDbContext();
            _mapper = mapper;
            _whatsappService = whatsappService;
            _smsService = smsService;
            _env = env;

        }
        public async Task<IResult> AddTicket(TicketModel tickets)
        {
            string projectRoot = AppContext.BaseDirectory;

            // No need to go 3 levels up. Stay inside /app directly.
            string folderPath = Path.Combine(projectRoot, "documents", "ticket_image");

            // Ensure the directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }


            try
            {
                Ticket ticket = new();
                ticket.Id = Guid.NewGuid();
                if (_forestDb.Tickets.Count() > 0)
                {
                    var ticketnumber = _forestDb.Tickets.Select(x => x.TicketId).Max();

                    ticket.TicketId = ticketnumber + 1;
                }
                else
                {
                    ticket.TicketId = 10001;
                }
                ticket.Title = tickets.Title;
                ticket.CameraId = Guid.Parse(tickets.CameraId);
                ticket.AnimalId = Guid.Parse(tickets.AnimalId);
                ticket.AreaId = Guid.Parse(tickets.AreaId);
                ticket.MemberId = Guid.Parse(tickets.MemberId);
                ticket.Type = tickets.Type;
                ticket.Remark = tickets.Remark;
                ticket.CreatedDate = DateTime.UtcNow.AddMinutes(330);
                ticket.Status = "Open";
                var cameraName = await _forestDb.Cameras.Where(m => m.Id == ticket.CameraId).Select(m => m.Name).FirstOrDefaultAsync();
                var AnimalName = await _forestDb.Animals.Where(m => m.Id == ticket.AnimalId).Select(m => m.Name).FirstOrDefaultAsync();
                var AreaName = await _forestDb.Areas.Where(m => m.Id == ticket.AreaId).Select(m => m.Name).FirstOrDefaultAsync();
                //if (tickets.folder != null && tickets.folder != "")
                //{
                //    var stream = new FileStream(tickets.folder, FileMode.Open, FileAccess.Read);
                //    var fileName = Path.GetFileName(tickets.folder);

                //    var formFile = new FormFile(stream, 0, stream.Length, "image", fileName)
                //    {
                //        Headers = new HeaderDictionary(),
                //        ContentType = "image/jpeg"
                //    };
                //    tickets.File = formFile;
                //}
                if (tickets.File != null)
                {
                    var extension = Path.GetExtension(tickets.File.FileName);
                    string fileName = ticket.Id.ToString() + extension;
                    SixLabors.ImageSharp.Image img = SixLabors.ImageSharp.Image.Load(tickets.File.OpenReadStream());
                    var imagePath = Path.Combine(folderPath, fileName);
                    img.Save(imagePath);
                    ticket.fileURL = "/GalleryAPI/Download?path=" + imagePath;
                }
                else
                {
                    ticket.fileURL = string.Empty;
                }
                await _forestDb.Tickets.AddAsync(ticket);
                await _forestDb.SaveChangesAsync();              
                var number = await _forestDb.Members.FindAsync(ticket.MemberId);
                SmsRequestModel smsRequest = new();
                smsRequest.Phone = number.PhoneNumber;
                await _smsService.SendMessage(smsRequest);
                await _forestDb.Messages.AddAsync(new Message
                {
                    Id = Guid.NewGuid(),
                    Sender = "UNOWAI",
                    Receiver = number.PhoneNumber,
                    TicketId = ticket.Id,
                    MessageText = "हिंदी தமிழ் ಕನ್ನಡ తెలుగు - UNIFYNOW AI LABS",
                    Timestamp = DateTime.UtcNow.AddMinutes(330)
                });
                await _forestDb.SaveChangesAsync();
                //if (tickets.File != null)
                //{
                //    await _smsService.SendTextMessage("+919958048761", "+"+ number.PhoneNumber, ticket.Remark);

                //    await _whatsappService.SendMedia(number.PhoneNumber, ticket.Remark, tickets.File,ticket.Id.ToString());
                //}
                //else
                //{
                //    await _smsService.SendTextMessage("+919958048761", "+" + number.PhoneNumber, ticket.Remark);

                //    await _whatsappService.SendTextMessage(number.PhoneNumber, ticket.Remark, ticket.Id.ToString());
                //}
                var response = await GetAllTicketsAsync();
                return Results.Ok(response);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<TicketModelRespose>> GetAllTicketsAsync()
        {
            try
            {
                List<TicketModelRespose> resposes = new();
                var data = await _forestDb.Tickets.Include(m => m.Animal).Include(m => m.Area).Include(m => m.Member).Include(m => m.Camera).OrderByDescending(m => m.CreatedDate).ToListAsync();
               foreach(var res in data)
                {
                    TicketModelRespose ticketModel = new();
                    ticketModel.Id = res.Id.ToString();
                    ticketModel.Title = res.Title;
                    ticketModel.TicketId = res.TicketId;
                    ticketModel.Camera = res.Camera.Name;
                    ticketModel.Animal = res.Animal.Name;
                    ticketModel.Area = res.Area.Name;
                    ticketModel.MemberId = res.MemberId.ToString();
                    ticketModel.Type = res.Type;
                    ticketModel.fileURL = res.fileURL;
                    ticketModel.CreatedDate = res.CreatedDate;
                    ticketModel.status = res.Status;
                    resposes.Add(ticketModel);
                }
               return resposes;
            }
            catch { throw; }
        }
        public async Task<List<MsgResponse>> GetAllMessagesAsync(string Id)
        {
            try
            {
                List<MsgResponse> resposes = new();
                var data = await _forestDb.Messages.Where(m => m.TicketId == Guid.Parse(Id)).OrderByDescending(m => m.Timestamp).ToListAsync();
                foreach (var res in data)
                {
                    MsgResponse response = new();
                    response.MessageText = res.MessageText;
                    response.TicketId = res.TicketId.ToString();
                    response.Timestamp = res.Timestamp;
                    resposes.Add(response);
                }
                return resposes;
            }
            catch { throw; }
        }
        public async Task<IResult> AddMessage(WhatsAppMsgRequest message)
        {
            try
            {
                if (message.File != null)
                {
                    await _whatsappService.SendMedia(message.Number, message.Message, message.File,message.ticketId.ToString());
                }
                else
                {
                    await _whatsappService.SendTextMessage(message.Number, message.Message, message.ticketId.ToString());
                }
                var result = await GetAllMessagesAsync(message.ticketId);
                return Results.Ok(result);
            }
            catch
            {
                throw;
            }
        }

    public async Task<IResult> PostStatus(StatusModel statusModel)
        {
            try
            {
                var result = _forestDb.Tickets.Find(Guid.Parse(statusModel.Id));
                if (statusModel.Status != "" && statusModel.Status == "Closed")
                {
                    result.Status = "Closed";
                }
                if(statusModel.MemberId !=  "")
                {
                    result.MemberId = Guid.Parse(statusModel.MemberId);
                }
                _forestDb.Tickets.Update(result);
                _forestDb.SaveChangesAsync();
                return Results.Ok("Success");
            }

            catch
            {
                throw;
            }
        }
    }
    public class StatusModel
    {
        public string Id { get; set; }

        public string? Status { get; set; }  

        public string? MemberId { get; set; }
    }

    public class TicketModel
        {
            public string Title { get; set; }

            public string CameraId { get; set; }

            public string AnimalId { get; set; }


            public string AreaId { get; set; }
            public string MemberId { get; set; }

            public string Type { get; set; }

            public string Remark { get; set; }

            public IFormFile? File { get; set; }

            //public string? folder { get; set; }


    }

    public class TicketModelRespose
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public Int64 TicketId { get; set; }
        public string Camera { get; set; }

        public string Animal { get; set; }

        public string Area { get; set; }
        public string MemberId { get; set; }

        public string Type { get; set; }

        public string fileURL { get; set; }

        public DateTime CreatedDate { get; set; }

        public string status { get; set; }

    }

    public class WhatsAppMsgRequest
    {
        public string Number { get; set; }

        public string Message { get; set; }
        public IFormFile? File { get; set; }

        public string ticketId { get; set; }
    }

    public class MsgResponse
    {
        public string MessageText { get; set; }

        public DateTime Timestamp { get; set; }     
        public string TicketId { get; set; }
    }
}
