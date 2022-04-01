# MonoControls
A visual content management library for MonoGame

The MonoControls library is a MonoGame library built to help manage and organise visual elements, operating on the idea of splitting them into stacks of visual layers, in which Resource loading, Drawing and Updating is automated.
The library offers a plethora of visual element objects, trying to abstract MonoGame's drawing logic, as well as allow for move advanced positioning, sizing and grouping, making it easier to animate, control and manage visuals.

### Getting started

To use the library, after cloning the repository in your project directory, you can simply add the available class library to you solution. All that you need to do after that is add it as a dependency and compile. Enjoy!

## Quick guide

Documentation is still scarce and like the library, still in construction.
To get you started with the basics, here is some brief info on the main objects of the lib. 
### Animatable

The `Animatable` class is the basic visual object in the library.
You can create one by calling its constructor and providing the necessary parameters.
```
anim = new Animatable(Content.Load("tex"), 0, 0, 32, 32, Color.White);
```

After creating the object, you need to draw it, in order for it to appear on screen:
```
anim.Draw(spriteBatch);
```

The library automatically handles all the drawing parameters, so you can easily animate objects by using their exposed properties and methods. 
Another feature of the `Animatable` class is that it can serve as a container for other Animatables, utilizing relative coordinates to their parent.
That functionality can be utilised by using the `Animatable` class as a list of other Animatables.
```
anim.Add(anim2);
```

Finally you can get some custom behaviour by extending the class or one of the other different types of Animatables provided.

### Screen

The `Screen` is the class provided that allows the stack management of visuals.
The `Screen` is an abstract class which provides the same structure as the base MonoGame Game class.

Similar to the base Game class, you need to extend it and overwrite the four game methods:

```
protected override void Resource_Load(ContentManager content_l = null){}
protected override void Current_Update(GameTime gameTime){}
protected override void Current_Draw(SpriteBatch spriteBatch){}
protected override void Dispose(){}
```

After that you can initialise your new class in the game class and call each method in its corresponding counterpart. 

The main functionality of the `Screen` class comes from the ability to stack such classes on top of one another.
That is done by assigning a Screen object to the nested field of the class.
```
screen1.nested = screen2;
```
Adding an object to a stack will mean it will automatically get its game methods called by its parent when needed.
To help you manage screen stacks, the static class ScreenHelpers offers some useful methods.

To allow screen to be moved around the window, the class provides a `customroot` object, which contains where the screen should be drawn relative to.
The `customroot` can be passed to Animatables as follows:

```
anim.Draw(spriteBatch, customroot);
```

## Continue Exploring

Even before the Documentation being complete I would encourage you to look through the different classes and see the methods and functionality they provide.
