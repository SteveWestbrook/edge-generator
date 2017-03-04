/**
 * proxy-generator test
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

const generator = require('../src/proxy-generator.js');
const path = require('path');

// This test will take the test assembly and generate/store a proxy for the specified type
describe('generator', () => {
  describe('#generate()', () => {
    it('should successfully generate a proxy for a .NET assembly', (done) => {      
      generator.generate(
        'DotNetTest.TestType1',
        path.resolve(
          __dirname,
          '..',
          'dotnet',
          'DotNetTest',
          'bin',
          'Debug',
          'DotNetTest.dll'),
        path.resolve(__dirname, '../junk'),
        done);
    });
  });
});
