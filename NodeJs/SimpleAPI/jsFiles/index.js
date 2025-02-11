const fs = require('fs')
const http = require('http');
const url = require('url');
const replaceTemplate = require(`${__dirname}/replaceData`)

//////////////////////////////////////////////
// FILE SYSTEM
/////////////////////////////////////////////
// //blocking code -> synchronouse way
// const textInput = fs.readFileSync('./txt/input.txt', 'utf-8');
// console.log(textInput);
// const textOutput = `This is what we know about the avocado: ${textInput}. \nCreated on ${Date.now()}`;
// fs.writeFileSync('./txt/input.txt', textOutput);

// //non-blocking, async way
// fs.readFile('./txt/start.txt', 'utf-8', (err, data1) => {
//     fs.readFile(`./txt/${data1}.txt`, 'utf-8', (err, data2) => {
//         console.log(data2);
//         fs.readFile(`./txt/append.txt`, 'utf-8', (err, data3) => {
//             console.log(data3)
//             fs.writeFile(`./txt/final.txt`, `${data2}\n${data3}`, 'utf-8', (err) => {
//                 console.log('Your file has been written');
//             })
//         })
//     })
// });
// console.log('Will read file');


//////////////////////////////////////////////
// SERVER
/////////////////////////////////////////////


const overview = fs.readFileSync(`${__dirname}/../overview.html`, 'utf-8');
const product = fs.readFileSync(`${__dirname}/../product.html`, 'utf-8');
const tempCards = fs.readFileSync(`${__dirname}/../template-card.html`, 'utf-8');

const apiRead = fs.readFileSync(`${__dirname}/../dev-data/data.json`, 'utf-8');
const dataObj = JSON.parse(apiRead);

const server = http.createServer((req, res) => {

    //console.log(req.url)
    //console.log(url.parse(req.url, true))
    const {query, pathname} = url.parse(req.url, true);

    //Overview
    if(pathname === '/' || pathname === '/overview')
    {
        res.writeHead(200, {'Content-type' : 'text/html'});
        const cardsHTtml = dataObj.map(el => replaceTemplate(tempCards, el)).join('');
        const output = overview.replace('{%PRODUCT_CARDS%}', cardsHTtml);

        res.end(output)
    }

    //Product
    else if(pathname === '/product')
    {
        //console.log(query);
        const productObj = dataObj[query.id];
        res.writeHead(200, {'Content-type' : 'text/html'});
        const output = replaceTemplate(product , productObj);

        res.end(output)
    }

    //API
    else if(pathname === '/api')
    {
        res.writeHead(200, {'content-type' : 'application/json'});
        res.end(apiRead);
    }

    //Not found
    else
    {
        res.writeHead(404, {
            'Content-type': 'text/html'
        });
        res.end('<h1>This is the UNKNOWN page</h1>');
    }
    
    //res.end('Hello from the server!!'); //sending back a response, use end
});

server.listen(8000, '127.0.0.1', () => {
    console.log('Server has started listgening to request on port 8000')
})