const fs = require('fs')
const express = require('express');

const app = express()
app.use(express.json()) // middeleware -> is just a function which can modify incoming data

const tours =JSON.parse(fs.readFileSync(`${__dirname}/dev-data/data/tours-simple.json`))

const getAllTours = (req, res) => {   //route handler
    res.status(200).json({
        status: 'success',
        results: tours.length,
        data: {
            tours //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

const getTourFromId = (req, res) => {   //route handler
    const id = req.params.id * 1

    if(id > tours.length)
    {
        return res.status(400).json({
            status: 'fail',
            message: 'Invalid ID'
        })
    }
    const tour = tours.find(el => el.id === id)
    res.status(200).json({
        status: 'success',
        data: {
            tour //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

const createTour = (req, res) => {
    console.log(req.body)
    const newId = tours[tours.length - 1].id + 1
    const newTour = Object.assign({id: newId}, req.body)

    tours.push(newTour)

    fs.writeFile(`${__dirname}/dev-data/data/tours-simple.json`, JSON.stringify(tours), err => {
        res.status(201).json({
            status: 'success',
            data: {
                tour: newTour
            }
        })
    })
}

const updateTour = (req, res) => {
    const id = req.params.id * 1
    if(id > tours.length){
        return res.status(400).json({
        status: 'fail',
        message: 'Invalid ID'
        })
    }

    res.status(200).json({
        status: 'success',
        data: {
            tour: '<Update tour here...>'
        }
    })
}

const deleteTour =  (req, res) => {
    const id = req.params.id * 1
    if(id > tours.length){
        return res.status(400).json({
        status: 'fail',
        message: 'Invalid ID'
        })
    }

    res.status(204).json({
        status: 'sucess',
        data: null
    })
}

//app.get('/api/v1/tours', getAllTours) //we are using the routes now
//app.get('/api/v1/tours/:id', getTourFromId) //we are using the routes now
//app.patch('/api/v1/tours/:id', updateTour) //we are using the routes now
//app.delete('/api/v1/tours/:id', deleteTour) //we are using the routes now
//app.post('/api/v1/tours', createTour) //we are using the routes now

app
    .route('/api/v1/tours')
    .get(getAllTours)
    .post(createTour)


app
    .route('/api/v1/tours/:id')
    .get(getTourFromId)
    .patch(updateTour)
    .delete(deleteTour)

const port = 3000
app.listen(port, () => {
    console.log(`App running on port ${port}...`)
})