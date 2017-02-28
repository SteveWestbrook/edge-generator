/**
 * ProxyGenerator function
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

const edge = require('edge');
const fs = require('fs');
const path = require('path');

const proxyGenerator = edge.func(function() {
  /*
    #r "./dotnet/bin/Debug/EdgeReference.dll"

    using System;
    using System.Threading.Tasks;
    using EdgeReference;

    public class Startup {
      private class Result
      {
        public string name;
        public string script;
      }

      public async Task<dynamic> Invoke(dynamic input) {
        Action<string, string> completion =
          new Action<string, string>((name, script) => {
            Result output = new Result();
            output.name = name;
            output.script = script;

            input.callback(output);
          });

        ProxyGenerator.Generate(
          input.typeFullName,
          input.assemblyLocation,
          completion);

        return null;
      }
    }
  */
});

module.exports = {
  generate: generateProxy
};

function generateProxy(typeName, assemblyPath, targetDirectory, callback) {
  var parameters = {
    typeFullName: typeName,
    assemblyLocation: assemblyPath,
    callback: (result) => {

      if (!result) {
        console.error('Error - proxy generation returned invalid result.')
        return;
      }

      console.log('Generated proxy for %s.', result.name);

      var writePath = path.join(
        targetDirectory,
        result.name.replace('.', '-') + '.js');
      var stream = fs.createWriteStream(writePath);

      stream.on('open', () => {
        stream.write(result.script, 'utf8', () => {
          stream.end();
          console.log('Finished writing to %s.', writePath);
        });
      });

      stream.on('error', (err) => {
        console.error('Failed to write to %s:', writePath);
        console.error(err);
      });
    }
  }

  proxyGenerator(parameters, (err) => {
    if (err) {
      console.error(err);
    } else {
      console.log('Proxy generation complete for %s.', typeName);
    }

    if (callback) {
      callback(err);
    }
  });
}
