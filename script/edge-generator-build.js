const os = require('os');
const spawn = require('child_process').spawn;

var compiler;

if (os.platform() == 'win32') {
  compiler = 'csc';
} else {
  compiler = 'mcs';
}

var build = spawn('./script/edge-generator-build.sh', [compiler]);

build.stdout.on('data', (data) => {
  console.log(data.toString('utf-8'));
});

build.stderr.on('data', (data) => {
  console.error(data.toString('utf-8'));
});
