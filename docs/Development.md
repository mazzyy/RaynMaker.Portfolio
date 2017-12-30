
# Development

The overall architecture of this project follows the "Clean Architecture".

## UI technology

The frontend is developed with Vue.js.

You need to install node.js and npm from here: https://nodejs.org/en/

Check that npm is added to the path.

### Visual Studio

If you develop the UI with Visual Studio as well it is recommended to install

https://marketplace.visualstudio.com/items?itemName=MadsKristensen.VuejsPack-18329

## Backend technology 

The Backend is developed using Suave.IO

## Testing

Most tests are written in BDD style. These tests do not intend to test every possible path in the code (as we might try with classic unit tests) - 
instead these tests aim to be a living documentation of the logic implemented. Especially negative examples and "bad code paths" are rarely tested
within these tests.

We therefore also name the tests with "Specs" in the end.


