const mongoose = require('mongoose')

const tourSchema = new mongoose.Schema({ //create a new schema, that is the data in the database
    name: {
        type: String,
        required: [true, 'A tour must have a name'],
        unique: true,
        trim: true
    },
    durations: {
        type: Number,
        require: [true, 'A tour must have a duration']
    },
    maxGourpSize:{
        type: Number,
        require: [true, 'A tour must have a maximum group size']
    },
    difficulty: {
        type: String,
        require: [true, 'A tour must have a duration']
    },
    ratingsAverage: { 
        type: Number,
        default: 4.5
    },
    ratingsQuantity: { 
        type: Number,
        default: 0
    },
    price: {
        type: Number,
        require: [true, 'A tour must have a price']
    },
    discount: {
        type: Number
    },
    summary: {
        type: String,
        trim: true,
        require: [true, 'A tour must have a summary']
    },
    description: {
        type: String,
        trim: true
    },
    imageCover: {
        type: String,
        require: [true, 'A tour must have a cover image']
    },
    images: [String],
    createdAt: {
        type: Date,
        default: Date.now()
    },
    startDates: [Date],
});

const Tour = mongoose.model('Tour', tourSchema) // creating a model out of the shema, modelname should start with UPPERCASE


module.exports = Tour;