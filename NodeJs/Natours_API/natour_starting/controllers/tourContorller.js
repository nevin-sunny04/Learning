const Tour = require('./../models/tourModel')

//We will now be importing from DB
//const tours =JSON.parse(fs.readFileSync(`${__dirname}./../dev-data/data/tours-simple.json`))


//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//Tours route Handler
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

exports.getAllTours = async (req, res) => 
    {   //route handler
    try 
    {

        //1. FILTERING
        const queryObj = {...req.query}
        const excludedFields = ['page', 'sort', 'limit', 'fields']
        excludedFields.forEach(el => delete queryObj[el])
        
        
        //2.Advanced Filtering
        let queryStr = JSON.stringify(queryObj)
        queryStr = queryStr.replace(/\b(gte|gt|lte|lt)\b/g, match => `$${match}`)
        console.log(JSON.parse(queryStr))


        const query = Tour.find(JSON.parse(queryStr));

        //THIS IS THE MONGOOSE METHOD
        //const tours = await Tour.find().where('difficulty').equals('easy')

        const tours = await query;
        res.status(200).json
        ({
        status: 'success',
        results: tours.length,
        data: 
        {
            tours //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })}
    catch (err){
        console.log(err)
        res.status(400).json({
        status: 'failed',
        message: 'Error could not be retrieved'
    })}
}

exports.getTourFromId = async (req, res) => {   //route handler
    try {
    const tours = await Tour.findById(req.params.id) //Tour.findOne({_id: req.params.id}) only returns only one document.
    res.status(200).json({
        status: 'success',
        data: {
            tours //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}
catch(err){
    res.status(400).json({
        status: 'failed',
        message: 'Error could not be retrieved',
    })
}
}

exports.createTour = async (req, res) => {
    try {
        const newTour = await Tour.create(req.body)
        console.log(req.body)
        res.status(201).json({
            status: 'success',
            data: {
                tour: newTour
            }
        })
    }
    catch (err){
        res.status(400).json({
            status: 'failed',
            message: 'Error could not be added'
        })
    }
}

exports.updateTour = async (req, res) => {
    try{
        const tours = await Tour.findByIdAndUpdate(req.params.id, req.body,
        {
            new: true
        })
        res.status(200).json({
        status: 'success',
        data: {
            tour: tours
        }
    })
}
catch (err){
    res.status(400).json({
    status: 'failed',
    message: 'Error could not be retrieved'
})}
}

exports.deleteTour = async (req, res) => {
    await Tour.findByIdAndDelete(req.params.id)
    res.status(204).json({
        status: 'sucess',
        data: null
    })
}
