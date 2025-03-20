const express = require('express');
const morgan = require('morgan')

const tourRouter = require('./routers/tourRoutes')
const userRouter = require('./routers/userRoutes')

const app = express()

if(process.env.NODE_ENV === 'development'){
    //using 3rd party middleware (morgan)
    app.use(morgan('dev'))
}
app.use(express.json()) // middeleware -> is just a function which can modify incoming data
app.use(express.static(`${__dirname}/public`))
//creating own middleware
app.use((req, res, next) => {
    console.log('Hello from the middleware')
    next() //always call next() in middleware else it will loop endlessly in the cycle
})

//another own middleware
app.use((req, res, next) => {
    req.requestTime = new Date().toISOString()
    next()
})

//app.get('/api/v1/tours', getAllTours) //we are using the routes now
//app.get('/api/v1/tours/:id', getTourFromId) //we are using the routes now
//app.patch('/api/v1/tours/:id', updateTour) //we are using the routes now
//app.delete('/api/v1/tours/:id', deleteTour) //we are using the routes now
//app.post('/api/v1/tours', createTour) //we are using the routes now

app.use('/api/v1/tours', tourRouter)
app.use('/api/v1/user', userRouter)

module.exports = app;