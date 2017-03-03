#!/usr/bin/env node
/**
 * edge-generator executable file
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const generator = require('../src/proxy-generator.js');
const process = require('process');
const path = require('path');
const args = require('node-args');

/**
 * command line:
 * edge-generator -a assembly [-t <target directory>] Namespace.Class1 Namespace.Class2
 * 
 */

if (!args.a) {
  console.error('No assembly specified.');
  return;
}

var directory = process.cwd();
var assembly = path.resolve(directory, args.a);
var writedirectory;

if (args.t) {
  writedirectory = path.resolve(directory, args.t);
} else {
  writedirectory = './'; 
}

var names = args.additional;
var errcount = 0;

// Start generating proxies.
constructClass();

function constructClass(err) {

  if (err) {
    errcount++;
  }

  // If no names remain, we are done.
  if (!names.length) {
    // Tell us about any errors
    if (errcount) {
      console.error('Proxy generation finished with %d errors.', errcount);
    } else {
      // Happy path
      console.log('Proxy generation finished.');
    }

    return;
  }

  var name = names.shift();

  // Generate a proxy; once finished, peel off the next item and repeat.
  generator.generate(name, assembly, writedirectory, constructClass);
}
