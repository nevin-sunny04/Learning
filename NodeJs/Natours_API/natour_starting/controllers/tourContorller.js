const fs = require('fs')

const tours =JSON.parse(fs.readFileSync(`${__dirname}./../dev-data/data/tours-simple.json`))
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//Tours route Handler
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

exports.checkId = (req, res, next, val) => {
    console.log(`Tour id is: ${val}`)
    if(req.params.id > tours.length) {
        return res.status(400).json({
            status: 'fail',
            message: 'Invalid ID'
        })
    }
    next();
}

exports.checkBody = (req, res, next) => {
    if (!req.body.name || !req.body.price) {
      return res.status(400).json({
        status: 'fail',
        message: 'Missing name or price'
      });
    }
    next();
  };

exports.getAllTours = (req, res) => {   //route handler
    console.log(req.requestTime)
    res.status(200).json({
        status: 'success',
        requestedAt: req.requestTime,
        results: tours.length,
        data: {
            tours //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

exports.getTourFromId = (req, res) => {   //route handler
    const id = req.params.id * 1
    const tour = tours.find(el => el.id === id)
    res.status(200).json({
        status: 'success',
        data: {
            tour //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

exports.createTour = (req, res) => {
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

exports.updateTour = (req, res) => {
    res.status(200).json({
        status: 'success',
        data: {
            tour: '<Update tour here...>'
        }
    })
}

exports.deleteTour =  (req, res) => {
    res.status(204).json({
        status: 'sucess',
        data: null
    })
}
