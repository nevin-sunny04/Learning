const dotenv = require('dotenv')
const fs = require('fs')
const Tour = require('./../../models/tourModel')
const mongoose = require('mongoose')
dotenv.config({path: './config.env'})
const { doc } = require('prettier')
//console.log(process.env)

const DB = process.env.DATABASE.replace('<PASSWORD>', process.env.DATABASE_PASSWORD)
mongoose.connect(DB, {})
.then(() => {
        console.log('DB connection successful')})

//READ JSON FILE
const tours = JSON.parse(fs.readFileSync(`${__dirname}/tours-simple.json`, 'utf-8'))

//import data into DB
const importData = async () => 
    {
try
{
    await Tour.create(tours)
    console.log('Data successfully loaded!')
    process.exit();
}
catch (err)
{
    console.log(err)
}}

        //DELETE ALL DATABASE
        const deteleData = async () => {
            try
            {
                await Tour.deleteMany()
                console.log('Data successfully deleted!')
                process.exit();
            }
            catch (err)
            {
                console.log(err)
            }
        }

        if(process.argv[2] === '--import') {
            importData();
        }
        if(process.argv[2] === '--delete') {
            deteleData()
        }