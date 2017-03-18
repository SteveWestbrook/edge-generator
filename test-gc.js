/**
 * A test of garbage collection to ensure that unused proxies are removed from
 * the .NET storage component that prevents them from being gc'd in .NET.
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const TestType1 = require('./test/DotNetTest-TestType1.js');
const TestType2 = require('./test/DotNetTest-TestType2.js');
const edge = require('edge');
const assert = require('assert');
const process = require('process');

/**
 * Gets a count of references currently stored in .NET, made available to JS
 */
var ReferenceCount = edge.func({ source: () => {/*
  #r "./bin/EdgeReference.dll"

  using System.Threading.Tasks;

  public class Startup {
    public async Task<object> Invoke(object input) {
      return EdgeReference.ReferenceManager.Instance.Count;
    }
  }
*/}});

// Get the number of references currently declared.
var beforeCount = ReferenceCount(null, true);
var i = 0;
var parent = {};

// Create a bunch of references, then get rid of them
while (++i < 1000) {
  parent.tt1 = new TestType1();

  // Get the number of references currently stored
  var afterCount = ReferenceCount(null, true);

  assert.ok(afterCount);
  assert.notEqual(afterCount, beforeCount);

  delete parent.tt1;
}

// Force garbage collection - this should clear out all those references
global.gc();

// Wait a little while
setTimeout(() => {
  // See how many references are now present
  var lastCount = ReferenceCount(null, true);

  // Make sure that the current number of references matches the number we  
  // started with. 
  assert.ok(lastCount <= beforeCount);

}, 1000);

