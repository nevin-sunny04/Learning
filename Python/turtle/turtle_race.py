import random
from turtle import Turtle, Screen

is_race_on = False
screen = Screen()
screen.setup(width=500, height=600)

user_bet = screen.textinput(title="Make your bet", prompt="Which turtle will win?")

color_list = ['red', 'orange', 'yellow', 'green', 'blue', 'violet']
all_turtle = []

x = -250
y = -100

for turtle_index in range(6):
    tim = Turtle(shape='turtle')
    tim.color(color_list[turtle_index])
    tim.penup()
    y += 50
    tim.goto(x=-250, y=y)
    all_turtle.append(tim)

if user_bet:
    is_race_on = True

while is_race_on:
    for turtle_index in all_turtle:
        if turtle_index.xcor() > 230:
            winning_color = turtle_index.pencolor()
            if winning_color == user_bet:
                print("You win")
            else:
                print("You lose")
            is_race_on = False
        random_distance = random.randint(0, 10)
        turtle_index.forward(random_distance)

screen.exitonclick()
