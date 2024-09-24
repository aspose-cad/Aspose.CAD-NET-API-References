const http = require('http');
const compression = require('compression');
const express = require('express');
const path = require('path');
const fs = require('fs');
const mime = require('mime-types');
const url = require('url');

const rootNs = 'aspose.cad'
//const rootNs = 'system.composition.convention'

const MAX_AGE = 86400 // 1 day

const KNOWN_FILE_TYPES = [
    '.html', '.css', '.js', '.png', '.jpg', '.jpeg', '.gif', 
	'.svg', '.ico', '.json', '.ts', '.map', '.scss', '.woff', 
	'.woff2', '.ttf', '.otf', '.eot', '.xml', '.html'
];

const rootDir = path.resolve(__dirname, '../_site');

function withoutQuery(urlString) {
    const parsedUrl = url.parse(urlString);
	//console.log(JSON.stringify(parsedUrl, null, 2));
    return parsedUrl.pathname != null ? `${parsedUrl.pathname}` : '';
}

const app = express();

app.use(compression());

app.use((req, res) => {
	console.log('=== Initial: ' + req.url);
	
    if (req.url.startsWith('/cad/net')) {
        req.url = req.url.replace('/cad/net', '');
	}
	
	if (req.url == '/aspose/' || req.url == '/aspose') {
        console.log('Redirect');
        res.writeHead(301, { 'Location': '/cad/net/aspose.cad/' });
        res.end();
        return;
	}
	
	if (req.url == '') {
        console.log('Redirect');
        res.writeHead(301, { 'Location': '/cad/net/' });
        res.end();
        return;
	}
	
	req.url = withoutQuery(req.url);
	req.url = req.url.replace(/^\//g, '');
	
	if (req.url.includes('..')) {
		console.log('HACK ATTEMPT');
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('Not Found');
        return;
	}
	
	///////
	if (!req.url || req.url == '/' || req.url == 'index') {
        req.url = 'index.html';
    }
	//////
	else if (req.url.endsWith('toc.html') && req.url != 'toc.html') {
		req.url = 'api/toc.html';
	}
	//////
	else if (req.url.endsWith('toc.json') && req.url != 'toc.json') {
		req.url = 'api/toc.json';
	}
	///////
    else if (req.url.startsWith(rootNs)) {
		if (!req.url.endsWith('/')) {
			console.log('Redirect');
            res.writeHead(301, { 'Location': req.url + '/' });
            res.end();
			return;
		}
		
		req.url = req.url.replace(/\//g, '.');
		req.url = 'api/' + req.url + 'html';
	}
	//////
	//else if (!KNOWN_FILE_TYPES.includes(path.extname(req.url))) {
	//	console.log('Redirect');
    //    res.writeHead(301, { 'Location': req.url + '/' });
    //    res.end();
    //    return;
    //}
	
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
        res.writeHead(200, 
		{ 
		    'Content-Type': contentType,
            'Cache-Control': KNOWN_FILE_TYPES.includes(path.extname(filePath)) 
			                     ? ('public, max-age=' + MAX_AGE) 
								 : 'no-cache'
		});

        const readStream = fs.createReadStream(filePath);
        readStream.pipe(res);
    });
});

const server = http.createServer(app);

console.log('Proxy server listening on port 8081');
server.listen(8081);