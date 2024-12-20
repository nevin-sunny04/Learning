package main

func main() {
	//cards := newDeck()
	//cards := cards.toByteAndSave("mY_cards")

	cards := newDeckFromFile("mY_cards")
	cards.printCards()
	cards.shuffle()
	cards.printCards()
	//hand, remainCards := deal(cards, 5)

}
