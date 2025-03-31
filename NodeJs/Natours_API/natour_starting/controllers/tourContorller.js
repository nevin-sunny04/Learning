const Tour = require('./../models/tourModel')
const APIFeature = require('./../utils/apiFeatures')

//middleware to edit the query for aliasing
exports.aliasTopTours = (req, res, next) => {
    req.query.limit = '5';
    req.query.sort = '-price';
    req.query.fields = 'name,price, ratingsAverage,summary,difficulty';
    next();
}
//We will now be importing from DB
//const tours =JSON.parse(fs.readFileSync(`${__dirname}./../dev-data/data/tours-simple.json`))


//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//Tours route Handler
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
exports.getAllTours = async (req, res) => 
    {   //route handler
    try 
    {
        const features = new APIFeature(Tour.find(), req.query)
            .filter()
            .sort()
            .limitFields()
            .pagination();
        const tours = await features.query;
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

exports.getTourStats = async (req, res) => {
    try{
        const stats = await Tour.aggregate([
            {
                $match : { ratingsAverage: { $gte: 4.5} }
            },
            {
                $group: {
                    _id: { $toUpper: '$difficulty' },
                    num: {$sum :1},
                    numRating: { $sum: '$ratingsQuantity'},
                    avgRating: { $avg: '$ratingsAverage'},
                    avgPrice: { $avg: '$price'},
                    minPrice: { $min: '$price'},
                    maxPrice: { $max: '$price'}
                }
            },
            {
                $sort: { avgPrice: 1}
            }
        ]);
        res.status(200).json
        ({
        status: 'success',
        data: 
        {
            stats //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }});
    }
    catch (err){
        console.log(err)
        res.status(400).json({
        status: 'failed',
        message: 'Error could not be retrieved'
    })}
}

exports.getMonthlyPlan = async (req, res) => {
    try {
      const year = req.params.year * 1; // 2021
  
      const plan = await Tour.aggregate([
        {
          $unwind: '$startDates'
        },
        {
          $match: {
            startDates: {
              $gte: new Date(`${year}-01-01`),
              $lte: new Date(`${year}-12-31`)
            }
          }
        },
        {
          $group: {
            _id: { $month: '$startDates' },
            numTourStarts: { $sum: 1 },
            tours: { $push: '$name' }
          }
        },
        {
          $addFields: { month: '$_id' }
        },
        {
          $project: {
            _id: 0
          }
        },
        {
          $sort: { numTourStarts: -1 }
        },
        {
          $limit: 12
        }
      ]);
  
      res.status(200).json({
        status: 'success',
        data: {
          plan
        }
      });
    } catch (err) {
      res.status(404).json({
        status: 'fail',
        message: err
      });
    }
};