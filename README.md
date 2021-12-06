# toolin

Hiya :) Thanks for reading this. 

To set up this package, there are a few steps to be followed. 

> Create an empty Game Object, name it StyleManager and give it the same tag (StyleManager).
> Add the Style ManagerScript as a new component. 
> Define the size of the array, these will hold your button names, tile icons and the prefabs that
will be instantiated to create the map in game. 
> Drag and drop the tile icons and the Map Parts (prefabs) individually into each element spot.
Element 0 should be your 'Empty' icon and prefab, as these are used to erase the map. 
>Ensure all prefabs to be instantiated each individually have the PartsScript attached to them. 
(nothing needs to be changed in the inspector) 
> Create an empty game object and call it Map, and give it the same tag (Map).  

To use the tool, go to Window > Level Editor Tool > Select a Tile style (this will default to whatever
is in your element 1 slot on the style manager. 
You'll be able to drag in a paint like fashion over the white grid, whereas holding down the
mouse button on the background grid will drag it so you can relocate/centre the grid. 


