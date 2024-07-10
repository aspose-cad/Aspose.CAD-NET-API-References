const http = require('http');
const httpProxy = require('http-proxy');

const proxy = httpProxy.createProxyServer({});
const server = http.createServer((req, res) => {
    if (req.url.startsWith('/cad/net')) {
        req.url = req.url.replace('/cad/net', '');
        proxy.web(req, res, { target: 'http://localhost:8080' });
    } else {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('Not Found');
    }
});

console.log('Proxy server listening on port 8081');
server.listen(8081);



// const express = require('express');
// const path = require('path');
// const app = express();

// const PORT = 8080;
// const SITE_DIR = '..\\_site';

// app.use('/cad/net', express.static(SITE_DIR));

// app.listen(PORT, () => {
  // console.log(`Server is running at http://localhost:${PORT}/cad/net`);
// });
