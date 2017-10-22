const { exec } = require('child_process');
const child = exec('npm start');

child.stdout.on('data', (chunk) => {
  console.log(chunk);
});
child.stderr.on('data', (chunk) => {
  console.log(chunk);
});
