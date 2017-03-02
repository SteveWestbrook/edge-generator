const TestType1 = require('./test/DotNetTest-TestType1.js');
const TestType2 = require('./test/DotNetTest-TestType2.js');
const edge = require('edge');
const assert = require('assert');
const process = require('process');

var ReferenceCount = edge.func({ source: () => {/*
  #r "./dotnet/bin/Debug/EdgeReference.dll"

  using System.Threading.Tasks;

  public class Startup {
    public async Task<object> Invoke(object input) {
      return EdgeReference.ReferenceManager.Instance.Count;
    }
  }
*/}});


var beforeCount = ReferenceCount(null, true);
var i = 0;
var parent = {};

while (++i < 1000) {
  parent.tt1 = new TestType1();
  var afterCount = ReferenceCount(null, true);

  assert.ok(afterCount);
  assert.notEqual(afterCount, beforeCount);

  delete parent.tt1;
}

global.gc();

setTimeout(() => {
  var lastCount = ReferenceCount(null, true);
  console.log(lastCount);
  console.log(beforeCount);
  console.log(afterCount);
  assert.ok(lastCount <= beforeCount);
}, 1000);

