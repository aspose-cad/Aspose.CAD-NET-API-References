const http = require('http');
const compression = require('compression');
const express = require('express');
const path = require('path');
const fs = require('fs');
const mime = require('mime-types');const url = require('url');

const rootNs = 'aspose.cad'
//const rootNs = 'system.composition.convention'

const knownFileTypes = [
    '.html', '.css', '.js', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.ico', 
	'.json', '.ts', '.map', '.scss', '.woff', '.woff2', '.ttf', '.otf', '.eot'
];

const rootDir = path.resolve(__dirname, '../_site');

function withoutQuery(urlString) {
  const parsedUrl = url.parse(urlString);
  return `${parsedUrl.pathname}`;
}

const app = express();

app.use(compression());

app.use((req, res) => {
	console.log('=== Initial: ' + req.url);
	
    if (req.url.startsWith('/cad/net')) {
        req.url = req.url.replace('/cad/net', '');
	}
	
	req.url = withoutQuery(req.url);
	req.url = req.url.replace(/^\/|\/$/g, '');
	
	///////
	if (req.url == '/' || req.url == '') {
		req.url = 'index.html';
	}
	//////
	if (req.url.endsWith('toc.json.html') && req.url != 'toc.json.html') {
		req.url = 'api/toc.json.html';
	}
	//////
	else if (req.url.endsWith('toc.json') && req.url != 'toc.json') {
		req.url = 'api/toc.json';
	}
	///////
    else if (req.url.startsWith(rootNs)) {
		req.url = req.url.replace(/\//g, '.');
		req.url = req.url + '.html';
		
		if (!req.url.startsWith('index')) {
			req.url = 'api/' + req.url;
		}
	}
	//////
	else if (!knownFileTypes.includes(path.extname(req.url))) {
		console.log('Redirect');
        res.writeHead(302, { 'Location': req.url + '/' });
        res.end();
        return;
    }
	
	const filePath = path.join(rootDir, req.url);

    fs.stat(filePath, (err, stats) => {
		console.log('Reading: ' + filePath);
		
        if (err || !stats.isFile()) {
			console.log('NOT FOUND');
            res.writeHead(404, { 'Content-Type': 'text/plain' });
            res.end('Not Found');
            return;
        }
		
		console.log('OK');
		
		const contentType = mime.lookup(filePath) || 'application/octet-stream';
        res.writeHead(200, { 'Content-Type': contentType });

        const readStream = fs.createReadStream(filePath);
        readStream.pipe(res);
    });
});

const server = http.createServer(app);

console.log('Proxy server listening on port 8081');
server.listen(8081);