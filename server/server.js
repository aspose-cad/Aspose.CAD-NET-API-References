const express = require('express');
const path = require('path');
const app = express();

const PORT = 8080;
const SITE_DIR = '..\\_site';

app.use('', express.static(SITE_DIR));

app.listen(PORT, () => {
  console.log(`Server is running at http://localhost:${PORT}`);
});
