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

const distdir = '../templates2'

const loader = {
  '.eot': 'file',
  '.svg': 'file',
  '.ttf': 'file',
  '.woff': 'file',
  '.woff2': 'file'
}

build()
  .then(() => {
    console.log('Build finished successfully')
  })
  .catch(err => {
    console.error(err)
    process.exit(1)
  })

async function build() {

  await Promise.all([buildAsposeModernTemplate()])

  copyToDist()
}

async function buildAsposeModernTemplate() {
  console.log('buildAsposeModernTemplate');	
	
  const config = {
    bundle: true,
    format: 'esm',
    splitting: true,
    minify: true,
    sourcemap: true,
    outExtension: {
      '.css': '.min.css',
      '.js': '.min.js'
    },
    outdir: 'aspose-modern/dist',
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
}

function copyToDist() {

  console.log('copyToDist');
  rmSync(distdir, { recursive: true, force: true })
  cpSync('aspose-modern', join(distdir, 'aspose-modern'), { recursive: true, overwrite: true, filter })

  function filter(src) {
    const segments = src.split(/[/\\]/)
    return !segments.includes('node_modules') && !segments.includes('package-lock.json') && !segments.includes('src')
  }

  function staticTocFilter(src) {
    return filter(src) && !src.includes('toc.html')
  }
}