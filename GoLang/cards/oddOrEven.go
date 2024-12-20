package main

import "fmt"

func createSliceOfNumber() []int {
	return []int{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
}

func printOddOrEven(nums []int) {
	for _, value := range nums {
		if value%2 == 0 {
			fmt.Println(value, "is even ")
		} else {
			fmt.Println(value, " is odd ")
		}
	}
}
