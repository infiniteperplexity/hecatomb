def ntiles(values, cells, sides, sidelen):
	tiles = {}
	strngs = []
	for i in range(values**cells):
		strngs.append(format(i,'#'+format(cells+2,'#003')+'b')[2:])

	for s in strngs:
		t = s
		duplicate = False
		symmetric = False
		rotations = sides

		for i in range(cells):
			t = t[1:] + t[:1]
			if t==t[:1]+t[1:][::-1]:
				symmetric = True

		for i in range(sides):
			t = t[sidelen-1:] + t[:sidelen-1]
			if t in tiles:
				duplicate = True

			if t[::-1] in tiles:
				duplicate = True

			if t==s:
				rotations-=1

		if not duplicate:
			if rotations==2:
				rotations = 1

			tiles[t] = {"symmetric": symmetric, "rotations": rotations}

	return tiles

tiles = ntiles(2, 8, 4, 3)

tally = 0
for tile in tiles.keys():
	tally = tally + 1 + tiles[tile]["rotations"]
	if tiles[tile]["symmetric"]==False:
		tally = tally + 1 + tiles[tile]["rotations"]
tally


#symmetric, no rotations
singletons = []
#symmetric, one rotation
toggles = []
#symmetric, three rotations
rotaters = []
#not symmetric, one rotation
flippers = []
#not symmetric, three rotations
frees = []

for tile, props in tiles.items():
	if props["symmetric"]:
		if props["rotations"]==0:
			singletons.append(tile)
		elif props["rotations"]==1:
			toggles.append(tile)
		else:
			rotaters.append(tile)
	else:
		if props["rotations"]==1:
			flippers.append(tile)
		else:
			frees.append(tile)


#24 that need to be rotated and flipped