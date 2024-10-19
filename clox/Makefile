build: src/*.c src/*.h
	clang -std=c99 -g3 -Wall -Wextra -Wshadow -Wdouble-promotion -fsanitize=address,undefined -Wstrict-prototypes -Wmissing-prototypes -pedantic src/*.c -o clox.out

gcc: src/*.c src/*.h
	gcc -std=c99 -g3 -Wall -Wextra -Wshadow -Wdouble-promotion -fsanitize=address,undefined -Wstrict-prototypes -Wmissing-prototypes -pedantic src/*.c -o clox.out
clean:
	rm -rf clox.out clox.out.dSYM

run: build
	./clox.out