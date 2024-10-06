# Lab2-MVC_Resto_Frontend


___
**Restaurant app (2/3), frontend. ASP.NET MVC.**  
Restaurant website with menu (all currently available meals divided in categories) and admin interface available after login. Admin can manage menu and bookings.  
___

Part 1, backend with ASP.NET Core Web Api:  
https://github.com/Magdagasikara/Lab1-WebAPI_Db_Resto  
Part 3, user's book a table component:  
https://github.com/Magdagasikara/Lab3-Resto_React  

Admin can book any time and any amount of customers (within restaurant's current capacity).  
User has limitations.  

Possible improvements:  
- menu printable  
- DealOfTheDay: some text if backend is down  
- improve accessibility   
- menu: add "vegetarian? click!" (extra filed needed in Meal models)  
- add to admin's view possibility to modify opening hours (now predefined array of arrays [[11,13], [17-23]])  
- look through viewmodels and validation messages  
- doublecheck breakpoint specific responsiveness in bootstrap  
- improve error management in controllers, add AccessDenied site  
- links to read through and consider:  
	- https://stackoverflow.com/questions/61672262/how-to-pass-an-object-to-an-action-method-in-asp-net-core-mvc  
	- https://stackoverflow.com/questions/62950905/net-mvc-how-in-a-url-action-to-send-an-object-in-an-object  
- improve terrible layout of forms: add meal, add reservation, edit...  
- how to get lazy loading of admin tabs  