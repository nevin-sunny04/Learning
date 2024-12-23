package main

import "fmt"

func main() {

	var colors map[string]string
	fmt.Println(colors)

	color := make(map[string]string)
	color["white"] = "#56edge"
	fmt.Println(color)

	colours := map[string]string{
		"red":   "#ff0000",
		"green": "#4bf745",
	}
	printMap(colours)
}

func printMap(c map[string]string) {
	for colorKey, hexValue := range c {
		fmt.Println("Hex code for", colorKey, "is", hexValue)
	}
}
