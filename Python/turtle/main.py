import random
import turtle
from turtle import Turtle, Screen

timmy_the_turtle = Turtle()
turtle.colormode(255)
timmy_the_turtle.shape('turtle')
color_tuple = (0.2, 0.8, 0.55)


def random_color():
    r = random.randint(0, 255)
    g = random.randint(0, 255)
    b = random.randint(0, 255)
    return (r, g, b)


for i in range(100):
    timmy_the_turtle.color(random_color())
    timmy_the_turtle.circle(100)
    timmy_the_turtle.setheading(timmy_the_turtle.heading() + 10)

screen = Screen()
screen.exitonclick()
