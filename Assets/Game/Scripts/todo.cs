    /*
    What is the right approach for building placement?
    
    1. flagMap of every service building placement
        overlap of those flagMaps for best house building placement

    2. flagMap of all houses unaffected by a particular service

    3. flagMap of all spaces with buildings / natural barriers (free or taken)
    ? OR list of all free spaces

    1 + 3 cascading rule for placing a house (from highest overlap to no service buildings)

    2 + 3 find empty space where service building area (square) covers highest number of unaffected houses

    farming and processing can be placed wherever (MAYBE introduce farmland / warehouse logic)

    gathering nearby resources 4. get flagMap for each already mined resource
     */

    /*

    1. citizen logic
        if need low -> check service availability (via house) -> is building constructed ->
         -> (check resource availability) -> (reserve resource purchase) -> go to service building
        go to work -> start working

    2. computer AI logic
        houses rather than workers should generate pressure to build services

        (house)

        (resource gathering)

        (processing)

        (service)
        check service availability -> pressure to build
        pressure to build reaches threshold -> construct building

        building construction available -> allocate builders and send workers
        building finished -> send workers

    3. temporal logic

        citizens:
        idle (default)
        walking (time is varied, cannot be precomputed)
        working (fixed duration)
        consuming (fixed duration)

    4. road generation

    5. graphics


    what is the interplay between needs fulfillment and worker growth?
    */

    /*
    1. write this module +
    2. check module load order +
    3. todos +
    4. refactor building templates +
    5. fill all game models with cubes +
    6. AI number of available workers +
    7. place starting house +
    8. DONT ACCUMULATE PRESSURE INDEFINITELY +
    9. create and display minimap +
    10. camera control (up, down, left, right, zoom in zoom out) +
    make minimap camera orthographic +
    strategic resources display on bottom panel +
    fix AI +
    upgrade houses logic +
    secondary resources display inside bottom right panel +
    building info on left click (bottom right panel) +
    citizen info on left click (bottom right panel) +
    clicking on nothing restores resource view +
    construction slider +
    camera rotation +

    perlin noise for iron, stone, salt deposits +
    textures for terrain and water
    3d models for buildings

    roads: shortest path vs already connected; shortest path to road network
    move camera on minimap click


    main menu
    pause menu
    save/load game
    */