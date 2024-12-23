package main

import "fmt"

type person struct {
	firstName string
	lastName  string
	contacInfo
}

type contacInfo struct {
	email   string
	zipCode int
}

func main() {
	alex := person{
		firstName: "Alex",
		lastName:  "Anderson",
		contacInfo: contacInfo{
			email: "alex@gmail.com", zipCode: 94000,
		},
	}
	alexPointer := &alex //memory address of alex. Not required, we can pass the root type itself and it will still work
	//alex.updateName("Alexa") this is how we use shortcut to do it using the copy of the root type
	alexPointer.updateName("Alenxandar")
	alex.printPersonName()
}

func (p person) printPersonName() {
	fmt.Printf("%v", p)
}

func (personUpdater *person) updateName(newFirstName string) { //a type of pointer that points at person
	(*personUpdater).firstName = newFirstName //value at that memory location which is going to be manipulated
}
