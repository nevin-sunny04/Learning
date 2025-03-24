const dotenv = require('dotenv')
const mongoose = require('mongoose')
dotenv.config({path: './config.env'})
const app = require('./app')
//console.log(process.env)

const DB = process.env.DATABASE.replace('<PASSWORD>', process.env.DATABASE_PASSWORD)
mongoose.connect(DB, {}).then(con => {
        console.log(con.connections)
        console.log('DB connection successful')
    })

//Starting server
const port = process.env.PORT
app.listen(port, () => {
    console.log(`App running on port ${port}...`)
})