// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

const esbuild = require('esbuild')
const { sassPlugin } = require('esbuild-sass-plugin')
const bs = require('browser-sync')
const { cpSync, rmSync } = require('fs')
const { join } = require('path')
const { spawnSync } = require('child_process')
const yargs = require('yargs/yargs')
const { hideBin } = require('yargs/helpers')
const argv = yargs(hideBin(process.argv)).argv

const loader = {
  '.eot': 'file',
  '.svg': 'file',
  '.ttf': 'file',
  '.woff': 'file',
  '.woff2': 'file'
}

build()
  .then(() => {
    console.log('templates build finished successfully')
  })
  .catch(err => {
    console.error(err)
    process.exit(1)
  })

async function build() {

  await Promise.all([buildAsposeModernTemplate()])
}

async function buildAsposeModernTemplate() {
  console.log('buildAsposeModernTemplate');	
	
  const config = {
    bundle: true,
    format: 'esm',
    splitting: true,
    minify: true, // false
    sourcemap: true,
    outExtension: {
      '.css': '.min.css',
      '.js': '.min.js'
    },
    outdir: 'aspose-modern/public',
    entryPoints: [
      'aspose-modern/src/docfx.ts',
      'aspose-modern/src/search-worker.ts',
    ],
    external: [
      './main.js'
    ],
    plugins: [
      sassPlugin()
    ],
    loader,
  }
  
  await esbuild.build(config)
  cpSync('aspose-modern/static', 'aspose-modern/public', { recursive: true, overwrite: true })
}
