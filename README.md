KSP_PartVolume 

This is a small mod which will add the ModuleCargoPart to all parts which don't already have it.  This is
necessary in order to allow a part to be an inventory cargo part that can be placed in inventories or allowing 
larger parts to be manipulated in EVA construction mode (but not placeable in inventories) the Part cfg file 
must have a ModuleCargoPart defined to it.

It works by using the stock methods GetRendererBounds() to get the outermost bounds of the part.

Usage



