print("Hello World")
total_ration = {"Milk": 2000, "Coffee": 2000, "Water": 2000}
list_of_options = [{"name": "Latte","Milk":100, "Coffee": 100, "Water": 100},
                   {"name": "Espresso","Milk":100, "Coffee": 500, "Water": 100},
                   {"name": "Capucino","Milk":500, "Coffee": 100, "Water": 100}]

request_from_customer = input("Enter your choice")


def minus_from_ration(i):
    for j in list_of_options:


i = 0
if(request_from_customer == 'Latte'):
    i = 0
elif(request_from_customer == 'Espresso'):
    i = 1
elif(request_from_customer == 'Capucino'):
    i = 2
else:
    print("Invalid Choice")
    SystemExit(0)
minus_from_ration(i)


