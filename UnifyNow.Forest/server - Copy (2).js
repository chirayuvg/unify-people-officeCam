const express = require('express');
const { Client, LocalAuth, MessageMedia, Location, Buttons } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const multer = require('multer');
const fs = require('fs');
const sql = require('mssql');
const mime = require('mime-types');
const path = require('path');
const swaggerUi = require('swagger-ui-express');
const swaggerJsDoc = require('swagger-jsdoc');
const cors = require('cors');
const os = require('os');
const axios = require('axios');

const messagesFilePath = path.join(__dirname, 'messages.json');

const getLocalIPAddress = () => {
    const interfaces = os.networkInterfaces();
    for (const interfaceName in interfaces) {
        for (const iface of interfaces[interfaceName]) {
            if (iface.family === 'IPv4' && !iface.internal) {
                return iface.address; // Return first non-internal IPv4 address
            }
        }
    }
    return '192.168.0.56'; // Fallback to localhost if no external IP is found
};

function getPublicIPAddress() {
    return axios.get('https://api64.ipify.org?format=json')
        .then(response => response.data.ip)
        .catch(error => {
            console.error('Error fetching public IP:', error);
            return '192.168.0.56'; // Fallback to localhost
        });
}



const localIP = getLocalIPAddress();
const port = 3000;

const app = express();
app.options('*', cors());

app.use(cors({
    origin: '*', // Allow all origins
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));


app.use(express.json());

const swaggerOptions = {
    swaggerDefinition: {
        openapi: '3.0.0',
        info: {
            title: 'WhatsApp API',
            description: 'API Documentation for WhatsApp Bot',
            version: '1.0.0',
        },
        servers: [{ url: `http://${localIP}:${port}` }],
    },
    apis: ['./server.js'], // Path to the API docs (this file)
};
const swaggerDocs = swaggerJsDoc(swaggerOptions);
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocs));


const storage = multer.diskStorage({
    destination: (req, file, cb) => {
        cb(null, 'uploads/'); // Ensure this folder exists
    },
    filename: (req, file, cb) => {
        cb(null, Date.now() + '-' + file.originalname);
    }
});


const upload = multer({
    storage: storage,
    limits: { fileSize: 50 * 1024 * 1024 }, // 50MB max size
    fileFilter: (req, file, cb) => {
        //  Allow all file types (images, PDFs, videos, etc.)
        cb(null, true);
    }
});

const uploadDir = path.join(__dirname, 'uploads');

if (!fs.existsSync(uploadDir)) {
    fs.mkdirSync(uploadDir, { recursive: true });
}

const storage2 = multer.diskStorage({
    destination: (req, file, cb) => {
        cb(null, uploadDir);
    },
    filename: (req, file, cb) => {
        cb(null, `${Date.now()}-${file.originalname}`);
    }
});

const upload2 = multer({
    storage: storage2,
    limits: { fileSize: 50 * 1024 * 1024 }, // 50MB max size
    fileFilter: (req, file, cb) => {
        //  Allow all file types (images, PDFs, videos, etc.)
        cb(null, true);
    }
});


const dbConfig = {
    user: process.env.SQL_USER || 'YOUR_USER',
    password: process.env.SQL_PASSWORD || 'YOUR_PASSWORD',
    server: process.env.SQL_HOST || 'YOUR_SQL_HOST',
    port: 1433,
    database: 'ForestDb',
    options: {
        encrypt: false, 
        trustServerCertificate: true
    }
};



// Initialize the WhatsApp client
const client = new Client({
    puppeteer: {
        headless: true,
        args: [
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-accelerated-2d-canvas",
            "--disable-gpu",
            "--window-size=1920x1080",
        ],
    },
    authStrategy: new LocalAuth(),
});


// Generate and scan this code with your phone
client.on('qr', qr => {
    qrcode.generate(qr, { small: true });
});

client.on('ready', () => {
    console.log('Client is ready!');
});

client.initialize();

// Middleware to parse JSON bodies
app.use(express.json());


// Ensure the downloads directory exists
const downloadDir = path.join(__dirname, 'downloads');

if (!fs.existsSync(downloadDir)) {
    fs.mkdirSync(downloadDir, { recursive: true });
}


if (!fs.existsSync(messagesFilePath)) {
    fs.writeFileSync(messagesFilePath, JSON.stringify({}, null, 2));
}

let messages = JSON.parse(fs.readFileSync(messagesFilePath, 'utf-8'));

function getAddressFromCoords(lat, lon) {
    return axios.get(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`)
        .then(response => {
            if (response.data && response.data.display_name) {
                return response.data.display_name; // Formatted address
            } else {
                return "Unknown location";
            }
        })
        .catch(error => {
            console.error("Error fetching address:", error);
            return "Unknown location";
        });
}


/**
 * @swagger
 * /simulate-message:
 *   post:
 *     summary: Simulate receiving a WhatsApp message with optional media
 *     description: This API simulates receiving a message from WhatsApp and processes it, including media files.
 *     requestBody:
 *       required: true
 *       content:
 *         multipart/form-data:
 *           schema:
 *             type: object
 *             properties:
 *               from:
 *                 type: string
 *                 description: Sender's WhatsApp number in international format with @c.us suffix
 *                 example: "911234567890@c.us"
 *               body:
 *                 type: string
 *                 description: Message text (ignored if a media file is sent)
 *                 example: "Hello, this is a test message."
 *               hasMedia:
 *                 type: boolean
 *                 description: Indicates if a media file is included
 *                 example: true
 *               media:
 *                 type: string
 *                 format: binary
 *                 description: Media file (image, video, document, etc.)
 *     responses:
 *       200:
 *         description: Message received and processed successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 status:
 *                   type: string
 *                   example: "Message processed successfully"
 *                 messageType:
 *                   type: string
 *                   example: "text"  # or "media"
 *                 mediaUrl:
 *                   type: string
 *                   example: "https://example.com/uploads/12345-image.jpg"
 *       400:
 *         description: Invalid request (e.g., missing required fields)
 *       500:
 *         description: Server error processing the message
 */


app.post('/simulate-message', upload.single('media'), async (req, res) => {
    try {
const { from, body, hasMedia, media } = req.body || {};

        // Ensure 'from' is valid before logging
        const senderNumber = (typeof from === 'string' && from) 
            ? from.replace('@c.us', '').replace(/[^a-zA-Z0-9 ]/g, '') 
            : 'unknown';
        
        console.log(`Simulated message from ${senderNumber}: ${body || 'Media Message'}`);

        if (!from) {
            return res.status(400).json({ error: "Sender 'from' is required" });
        }

        if (!messages[from]) {
            messages[from] = [];
        }

        let storedMessage = {
            text: body || '',
            timestamp: new Date().toISOString(),
        };

        const botNumber = client?.info?.wid?.user 
            ? client.info.wid.user.replace(/[^a-zA-Z0-9 ]/g, '') 
            : '';

        const receivernumber = botNumber;
        const messagetext = (typeof body === 'string' && body) 
            ? body.replace(/[^a-zA-Z0-9 ]/g, '') 
            : '';

        await sql.connect(dbConfig);

        await sql.query(`
            INSERT INTO Messages (Sender, Receiver, MessageText)
            VALUES ('${senderNumber}', '${receivernumber}', '${messagetext}')
        `);

        if (req.file) {
            try {
                const mimeType = req.file.mimetype || 'application/octet-stream';
                const extension = mime.extension(mimeType) || 'bin';
                const fileName = `${Date.now()}-${senderNumber}.${extension}`;
                const filePath = path.join(downloadDir, fileName);
            

                // Save file from buffer
                fs.renameSync(req.file.path, filePath);
                console.log(`Simulated media received and saved: ${filePath}`);

                storedMessage.media = filePath;
            } catch (error) {
                console.error('Error saving simulated media file:', error);
            }
        }
	messages[from] = messages[from] || [];

        messages[from].push(storedMessage);
        fs.writeFileSync(messagesFilePath, JSON.stringify(messages, null, 2));

        res.json({ status: 'Message processed successfully' });
    } catch (error) {
        console.error('Error processing message:', error);
        res.status(500).json({ error: 'Server error processing the message' });
    }
});


const googleMapsRegex = /@(-?\d+\.\d+),(-?\d+\.\d+),\d+z/;  // Updated regex to match Google Maps place links
const googleShortenedRegex = /https?:\/\/maps\.app\.goo\.gl\/[a-zA-Z0-9]+/;

const processGoogleMapsUrl = async (url, sender, receiver) => {
 //   const match = url.match(googleShortenedRegex);
const result = extractPlaceName(url);

//console.log(match);
  //  if (match) {
     //   const latitude = match[1];
     //   const longitude = match[2];
     //   const googleMapsUrl = `https://www.google.com/maps?q=${latitude},${longitude}`;
const messagetext = message.body.replace(/[^a-zA-Z0-9 ]/g, '');

        const textMessage = `Location Received!(${result})`;

	    await sql.connect(dbConfig);

    await sql.query(`
        INSERT INTO Messages (Sender, Receiver, MessageText)
        VALUES ('${sender}', '${receiver}', '${textMessage}')
    `);

        await client.sendMessage(`${receiver}`, textMessage);
   // } else {
   //     console.warn("No valid coordinates found in URL:", textMessage);
   // }
};


const processGoogleShortenedUrl = async (url, sender, receiver) => {
    try {
        // Expand the shortened Google Maps URL
        const response = await axios.head(url, { maxRedirects: 5 });
        const expandedUrl = response.request.res.responseUrl; // Get the final redirected URL
//console.log(expandedUrl);
        // Now process it as a normal Google Maps URL
console.log(1);
        await processGoogleMapsUrl(expandedUrl, sender, receiver);
    } catch (error) {
        console.error("Error expanding Google Maps shortened URL:", error);
    }
};

const extractPlaceName = (url) => {
    const match = url.match(/\/maps\/place\/([^\/?]+)/);
    if (match) {
        return decodeURIComponent(match[1]); // Decode URL encoding
    }
    return null; // No match found
};



//  **Fix: Ensure `mimetype` is Defined When Receiving Media**

client.on('message', async (message) => {
    console.log(` Received message from ${message.from}: ${message.body || 'Media Message'}`);


    if (!messages[message.from]) {
        messages[message.from] = [];
    }



const googleMapsRegex = /@(-?\d+\.\d+),(-?\d+\.\d+),\d+z/;  // Updated regex to match Google Maps place links
const googleShortenedRegex = /https?:\/\/maps\.app\.goo\.gl\/[a-zA-Z0-9]+/;

//console.log(googleMapsRegex,googleShortenedRegex);
const botNumber = client.info.wid.user.replace(/[^a-zA-Z0-9 ]/g, '');
let receivernumber = message.from; 
let senderNumber = botNumber;
const messagetext = message.body.replace(/[^a-zA-Z0-9 ]/g, '');


    if (message.body.match(googleMapsRegex)) {
console.log(2);
        processGoogleMapsUrl(message.body, senderNumber, receivernumber);
    }
    
    if (message.body.match(googleShortenedRegex)) {
console.log(3);
        processGoogleShortenedUrl(message.body, senderNumber, receivernumber);
    }

receivernumber = botNumber; 
senderNumber = message.from;

    let storedMessage = {
        text: message.body || '',
        timestamp: new Date().toISOString(),
    };

    
    await sql.connect(dbConfig);

    await sql.query(`
        INSERT INTO Messages (Sender, Receiver, MessageText)
        VALUES ('${senderNumber}', '${receivernumber}', '${messagetext}')
    `);




    // Handle Media Messages
    if (message.hasMedia) {
        try {
            const media = await message.downloadMedia();
            const mimeType = media.mimetype || 'application/octet-stream';
            const extension = mime.extension(mimeType) || 'bin';
            const fileName = `${Date.now()}-${message.from}.${extension}`;
            const filePath = path.join(downloadDir, fileName);

            // Convert base64 and save file
            const fileBuffer = Buffer.from(media.data, 'base64');
            fs.writeFileSync(filePath, fileBuffer);

            console.log(` Media received and saved: ${filePath}`);

            // Store media path in the message
            storedMessage.media = filePath;
        } catch (error) {
            console.error(' Error saving media file:', error);
        }
    }

    (async () => {

        if (message.location) {
    const latitude = message.location.latitude;
    const longitude = message.location.longitude;
    const googleMapsUrl = `https://www.google.com/maps?q=${latitude},${longitude}`;
    receivernumber = message.from; 
    senderNumber = botNumber;

    const textMessage = `📍 Location Received!\nLatitude: ${latitude}\nLongitude: ${longitude}\n[View on Google Maps](${googleMapsUrl})`;


        await sql.connect(dbConfig);

        // Use parameterized query to prevent SQL injection
        const query = `
            INSERT INTO Messages (Sender, Receiver, MessageText)
            VALUES (@sender, @receiver, @message)
        `;

        const request = new sql.Request();
        request.input('sender', sql.VarChar, senderNumber);
        request.input('receiver', sql.VarChar, receivernumber);
        request.input('message', sql.VarChar, textMessage);

        await request.query(query);


    await client.sendMessage(`${receivernumber}`, textMessage);
    }
})();


    // Append message to messages.json
    messages[message.from].push(storedMessage);
    fs.writeFileSync(messagesFilePath, JSON.stringify(messages, null, 2));

    // Auto-reply (Optional)
    if (message.body === '!ping') {
        message.reply('pong');
    }
});


/**
 * @swagger
 * /send-message:
 *   post:
 *     summary: Send a WhatsApp message
 *     description: Sends a text message to a given WhatsApp number.
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               number:
 *                 type: string
 *                 example: "1234567890"
 *               message:
 *                 type: string
 *                 example: "Hello, this is a test message."
 *     responses:
 *       200:
 *         description: Message sent successfully
 *       400:
 *         description: Bad request, missing parameters
 *       500:
 *         description: Error sending message
 */

app.post('/send-message', async (req, res) => {
    console.log("Received request body:", req.body);

    const { number, message } = req.body;

    // Validate input parameters
    if (!number || !message) {
        return res.status(400).json({ error: 'Number and message are required' });
    }

    try {
        // Clean the numbers and message text
        const botNumber = client.info.wid.user.replace(/[^a-zA-Z0-9]/g, '');
        const senderNumber = number.replace(/[^a-zA-Z0-9]/g, '');
        const receiverNumber = botNumber;
        const messageText = message.replace(/[^a-zA-Z0-9 ]/g, '');

        // Establish database connection
        await sql.connect(dbConfig);

        // Execute the database insert query inside a try-catch block
        try {
            await sql.query(`
                INSERT INTO Messages (Sender, Receiver, MessageText)
                VALUES ('${senderNumber}', '${receiverNumber}', '${messageText}')
            `);
        } catch (dbError) {
            console.error("Database insertion error:", dbError);
            return res.status(500).json({ error: "Failed to insert message into database" });
        }

        // Send the message via WhatsApp API
        await client.sendMessage(`${number}@c.us`, message);

        // Send success response
        res.status(200).json({ success: 'Message sent successfully' });
    } catch (error) {
        console.error('Error sending message:', error);
        res.status(500).json({ error: 'Error sending message' });
    } finally {
        // Close the database connection if open
        if (sql.connected) {
            await sql.close();
        }
    }
});


/**
 * @swagger
 * /send-media:
 *   post:
 *     summary: Send media via WhatsApp
 *     description: Sends an image, PDF, or any media file to a given WhatsApp number.
 *     consumes:
 *       - multipart/form-data
 *     requestBody:
 *       required: true
 *       content:
 *         multipart/form-data:
 *           schema:
 *             type: object
 *             properties:
 *               number:
 *                 type: string
 *                 example: "1234567890"
 *               caption:
 *                 type: string
 *                 example: "Here is your file"
 *               media:
 *                 type: string
 *                 format: binary
 *     responses:
 *       200:
 *         description: Media sent successfully
 *       400:
 *         description: Bad request, missing parameters
 *       500:
 *         description: Error sending media
 */

// ** Fixed: Endpoint to Send Media**
app.post('/send-media', upload2.single('media'), async (req, res) => {
    const { number, caption } = req.body;
    const file = req.file;

const botNumber = client.info.wid.user.replace(/[^a-zA-Z0-9 ]/g, '');
const senderNumber = number.replace(/[^a-zA-Z0-9 ]/g, '');
const receivernumber = botNumber;
const messagetext = file.path.replace(/[^a-zA-Z0-9 ]/g, '');

    await sql.connect(dbConfig);

    await sql.query(`
        INSERT INTO Messages (Sender, Receiver, MessageText)
        VALUES ('${senderNumber}', '${receivernumber}', '${messagetext}')
    `);



    if (!number || !file) {
        return res.status(400).send('Number and media file are required');
    }

    try {
        if (!fs.existsSync(file.path)) {
            return res.status(400).send('Uploaded file not found');
        }

        const fileBuffer = fs.readFileSync(file.path);
        const base64Data = fileBuffer.toString('base64');

        const mimeType = mime.lookup(file.originalname) || 'application/octet-stream';

        //  Send as a document (fixes WhatsApp download issues)
        const media = new MessageMedia(mimeType, base64Data, file.originalname);
        await client.sendMessage(number + '@c.us', media, { sendMediaAsDocument: true, caption: caption || '' });

        //  Delete file after sending
        fs.unlinkSync(file.path);

        res.status(200).send('Media sent successfully as a document');
    } catch (error) {
        console.error('Error sending media:', error);
        res.status(500).send('Error sending media');
    }
});

/**
 * @swagger
 * /messages:
 *   get:
 *     summary: Get all stored messages
 *     description: Retrieves all stored messages from messages.json.
 *     responses:
 *       200:
 *         description: Returns messages
 *       404:
 *         description: messages.json not found
 */

app.get('/messages', (req, res) => {
    if (fs.existsSync(messagesFilePath)) {
        const messages = fs.readFileSync(messagesFilePath, 'utf-8');
        res.setHeader('Content-Type', 'application/json');
        res.send(messages);
    } else {
        res.status(404).json({ error: 'messages.json not found' });
    }
});



/**
 * @swagger
 * /send-current-location:
 *   post:
 *     summary: Send current location via WhatsApp
 *     description: Sends the current location (latitude, longitude) along with the address to a given WhatsApp number.
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               number:
 *                 type: string
 *                 example: "1234567890"
 *               latitude:
 *                 type: number
 *                 format: double
 *                 example: 37.7749
 *               longitude:
 *                 type: number
 *                 format: double
 *                 example: -122.4194
 *     responses:
 *       200:
 *         description: Location sent successfully
 *       400:
 *         description: Bad request, missing parameters
 *       500:
 *         description: Error sending location
 */
app.post('/send-current-location', (req, res) => {
    
    const { latitude, longitude, number } = req.body;

    if (!number || !latitude || !longitude) {
        return res.status(400).json({ error: "Missing required fields" });
    }



const botNumber = client.info.wid.user.replace(/[^a-zA-Z0-9 ]/g, '');
const receiverNumber = number.replace(/[^a-zA-Z0-9 ]/g, '');
const senderNumber = botNumber;

    if (!number || !latitude || !longitude) {
        return res.status(400).send("Number, latitude, and longitude are required.");
    }

(async () => {
    try {
        const address = await getAddressFromCoords(latitude, longitude);
        const locationMessage = new Location(latitude, longitude, address);

        // Ensure locationMessage is a string before applying replace
        const newmessage = address.replace(/[^a-zA-Z0-9 ]/g, '');

        await sql.connect(dbConfig);

        // Use parameterized query to prevent SQL injection
        const query = `
            INSERT INTO Messages (Sender, Receiver, MessageText)
            VALUES (@sender, @receiver, @message)
        `;

        const request = new sql.Request();
        request.input('sender', sql.VarChar, senderNumber);
        request.input('receiver', sql.VarChar, receiverNumber);
        request.input('message', sql.VarChar, newmessage);

        await request.query(query);
        await client.sendMessage(`${receiverNumber}@c.us`, newmessage);

        console.log("Message successfully inserted into the database.");
    } catch (error) {
        console.error("Error:", error);
    }
})();
        res.status(200).json({ success: 'Message sent successfully as address' });

});


getPublicIPAddress().then(publicIP => {
    const port = 3000;
    const host = '0.0.0.0'; // Listen on all available interfaces

    const swaggerOptions = {
        swaggerDefinition: {
            openapi: '3.0.0',
            info: {
                title: 'WhatsApp API',
                description: 'API Documentation for WhatsApp Bot',
                version: '1.0.0',
            },
            servers: [{ url: `http://${localIP}:${port}` }],
        },
        apis: ['./server.js'],
    };

    const swaggerDocs = swaggerJsDoc(swaggerOptions);
    app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocs));

    app.listen(port, host, () => {
        console.log(`Server running at http://${localIP}:${port}`);
    });
}).catch(error => {
    console.error("Failed to get public IP:", error);
});