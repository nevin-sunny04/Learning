const fs = require('fs')

const users =JSON.parse(fs.readFileSync(`${__dirname}./../dev-data/data/users.json`))
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//Users route Handler
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

exports.checkId = (req, res, next, val) => {
    console.log(`User id is: ${val}`)
    if(req.params.id > users.length) {
        return res.status(400).json({
            status: 'fail',
            message: 'Invalid ID'
        })
    }
    next();
}

exports.getAllUsers = (req, res) => {
    console.log('Hello from users')
    res.status(200).json({
        status: 'success',
        requestedAt: req.requestTime,
        results: users.length,
        data: {
            users //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

exports.getUserFromId = (req, res) => {   //route handler
    const id = req.params.id * 1
    const user = users.find(el => el.id === id)
    res.status(200).json({
        status: 'success',
        data: {
            user //from ES6 we dont have to mention key and value if they have same name, else it will be tours: tours
        }
    })
}

exports.createUser = (req, res) => {
    console.log(req.body)
    const newId = users[users.length - 1].id + 1
    const newUser = Object.assign({id: newId}, req.body)

    tours.push(newUser)

    fs.writeFile(`${__dirname}${__dirname}/dev-data/data/users.json`, JSON.stringify(users), err => {
        res.status(201).json({
            status: 'success',
            data: {
                user: newUser
            }
        })
    })
}

exports.updateUser = (req, res) => {
    res.status(200).json({
        status: 'success',
        data: {
            user: '<Update tour here...>'
        }
    })
}

exports.deleteUser =  (req, res) => {
    res.status(204).json({
        status: 'sucess',
        data: null
    })
}