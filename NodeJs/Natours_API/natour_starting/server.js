const dotenv = require('dotenv')
const mongoose = require('mongoose')
dotenv.config({path: './config.env'})
const app = require('./app')
const { doc } = require('prettier')
//console.log(process.env)

const DB = process.env.DATABASE.replace('<PASSWORD>', process.env.DATABASE_PASSWORD)
mongoose.connect(DB, {}).then(con => {
        console.log('DB connection successful')
    })

// testTour.save().then(doc => {  //save the document to the database
//     console.log(doc)
// }).catch(err => {
//     console.log("Error :", err)
// })

//Starting server
const port = process.env.PORT | 3000
app.listen(port, () => {
    console.log(`App running on port ${port}...`)
})