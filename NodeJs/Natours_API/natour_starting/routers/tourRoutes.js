const express = require('express')
const tourController = require('./../controllers/tourContorller')

const router = express.Router()
//router.param('id', tourController.checkId)

//create a checkbody middleware
//check if body contains the name and price property
//if not, send back 400 (bad request)
//add it to the post handler stack

router
    .route('/top-5-cheap')
    .get(tourController.aliasTopTours, tourController.getAllTours) //incase we want to add a alias, we need to make use of a middleware to modify the query

router.route('/tour-stats')
    .get(tourController.getTourStats);

router.route('/monthly-plan/:year')
    .get(tourController.getMonthlyPlan);

router
    .route('/')
    .get(tourController.getAllTours)
    .post(tourController.createTour)

router
    .route('/:id')
    .get(tourController.getTourFromId)
    .patch(tourController.updateTour)
    .delete(tourController.deleteTour)

module.exports = router