namespace Unify.STR.Attendance.Data.SQL
{
    public class FilterSelectionViewModel
    {

        public Guid? DivisionId { get; set; }
        //public Division? Division { get; set; }
        public Guid? RangeId { get; set; }
        //public Range? Range { get; set; }
        public Guid? SectionId { get; set; }
        //public Section? Section { get; set; }
        public Guid? BeatId { get; set; }
        //public Beat? Beat { get; set; }
        public Guid? EmployeeId { get; set; }
        //public Employee? Employee { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}
