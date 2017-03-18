/**
 * ProxyGenerator function
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

'use strict';

const edge = require('edge');
const fs = require('fs');
const path = require('path');

module.exports = {
  generate: generateProxy
};

const bindir = path.resolve(__dirname, '../bin');

/**
 * Generates a proxy to a specified .NET object and any objects it depends on, 
 * either through inheritance or reference.  
 * 
 * @param input {object} Takes three parameters:
 *        typeFullName The name, including namespace of the type to be proxied.
 *        assemblyLocation The location of the assembly file that contains 
 *                         the type.
 *        callback A callback which is trigered each time a type proxy is 
 *                 generated.  It provides an output parameter with name and 
 *                 script members.
 */
const proxyGenerator = edge.func(
  `
    #r "${bindir}/EdgeReference.dll"
    #r "${bindir}/EdgeGenerator.dll"

    using System;
    using System.Threading.Tasks;
    using EdgeReference;
    using EdgeGenerator;

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
  `
);

function generateProxy(typeName, assemblyPath, targetDirectory, callback) {

  // parameters to be passed to the proxy generator
  var parameters = {
    typeFullName: typeName,
    assemblyLocation: assemblyPath,

    // Annoying non-standard callback from edge
    callback: (result) => {

      // There should always be a result
      if (!result) {
        console.error('Error - proxy generation returned invalid result.')
        return;
      }

      console.log('Generated proxy for %s.', result.name);

      var writePath = path.join(
        targetDirectory,
        result.name.replace('.', '-') + '.js');

      var stream = fs.createWriteStream(writePath);

      // Problem
      stream.on('error', (err) => {
        console.error('Failed to write to %s:', writePath);
        console.error(err);
      });
    
      // Open a file and write the new script into it
      stream.on('open', () => {
        stream.write(result.script, 'utf8', () => {
          stream.end();
          console.log('Finished writing to %s.', writePath);
        });
      });
    }
  }

  // Generate proxy
  proxyGenerator(parameters, (err) => {
    if (err) {
      console.error(err);
    }

    if (callback) {
      callback(err);
    }
  });
}
