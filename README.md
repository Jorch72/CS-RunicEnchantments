# Runic Enchantments 2D Language Interpreter

Runic script is a 2D interpreted language with multiple simultaneously processed instruction pointers. Each instruction pointer (IP) is spawned at an entry point with a mana ("energy") value, a direction, and an empty stack. Each cycle each IP (in order of creation) executes the instuction under itself and then moves forwards. If the instruction requires a quantity of mana and the IP does not have that much, then the update is treated as a NOP and the IP is not advanced. At the end of each update any pointers that exist in the same cell and have the same facing are merged. The older one retains its stack and gains the mana of the newer one, which is then destroyed. Any IP that has 0 or less mana is also destroyed.

## Commands

|        Rune       |           Name           | Description      |
|-------------------|--------------------------|------------------|
|` `| Empty | NOP - Nothing happens. IP will advance to the next cell. |
|`0`-`9`| Integer Literals | Pushes the single digit value onto the stack |
|`+` `-` `*` `,` `%`| Mathematical operators | Addition, Subtraction, Multiplication, Division, and Modulo. These also operate on two Vectors where `,` is a Dot Product and `*` is a Cross Product |
|`Z`| Negate | Multiplies the top value on the stack by -1 |
|`p`| Power | Pops two values `x` and `y` and pushes `x^y` (`x` raised to th3 power `y`) onto the stack (e.g. `>33p$;` will print `27`) |
|`>` `<` `^` `v`| Simple Entry | Spawns an IP with the indicated facing with 10 mana at program start. Acts as an empty space otherwise. |
|`$` `@`| Output | `$` prints the top value of the stack to Debug.Log. `@` dumps the entire stack |
|`;`| Terminator | Destroys an IP |
|`m`| Mana | Pushes the current mana value onto the stack |
|`M`| Min Mana | Acts as a threshold barrier. The top value of the stack is popped and compared to the IP's mana. If the mana is greater than the popped value, the IP advances. Otherwise the value is pushed back. |
|`\` `/` `\|` `_` `#`| Reflectors | Changes the direction of an IP |
|`U` `D` `L` `R`| Direction | Changes the pointers direction UP, DOWN, LEFT, RIGHT respectively. Can also use `↑` `↓` `←` `→` |
|`:`| Duplicator | Duplicates the top value on the stack |
|`~`| Pop | Pops the top value off the stack and discards it |
|`{` `}`| Rotate Stack | Rotates the entire stack left or right respectively |
|`S`| Swap | Swaps the top two values of the stack |
|`s`| Swap N | Pops a value off the stack, then pops that many values off the stack, rotates them right one, and pushes them back.  (e.g. if your stack is [1,2,3,4,3], calling `s` results in [1,4,2,3]) |
|`r`| Reverse | Reverses the stack |
|`l`| Length | Pushes the current length of the stack onto the stack |
|`=`| Equals | Pops `x` and `y` and pushes `1` if they are equal, `0` otherwise |
|`(` `)`| Less and Greater | Less than and Greater than respectively. Pushes `1` if true, pushes `0` otherwise |
|`!`| Trampoline | Skips the next instruction |
|`?`| Conditional Trampoline | Pops the top value of the stack. If it is non-zero (or non-null for reference types), skip the next instruction |
|`Q`| Nearby Objects | Pops a value `x` off the stack and calls `Physics.OverlapSphere(selfPosition, x)` pushing each returned GameObject onto the stack. Requires a minimum of `x`+1 mana to execute and consumes `x` mana. |
|`N`| Name | Pushes the name of a popped GameObject onto the stack |
|`t`| This | Pushes the GameObject representing `this` onto the stack (i.e. the GameObject the script is functionally running on) |
|`P`| Position | Pushes the Vector3 location of a popped GameObject onto the stack |
|`V`| Vector3 | Pops three values `x`,`y`,`z` off the stack and pushes a resulting Vector3 onto the stack |
|`d`| Distance | Pops two GameObject or Vector3 off the stack and computes their distance |

Any popped object that does not match the expected types is discarded, and the executed command is computed as best as possible (e.g. popping `(3,0,0)` and `5` off the stack when computing a distance `d` will result in a `3` being pushed onto the stack as a distance of `(3,0,0)` is `3` units: effectively results in pushing the vector's own magnitude).

IPs that advance off the edge of the program (in any direction) are moved to the far edge. In this way `>1$` will print an endless series of `1`s.

Currently all programs are terminated after 100 execution steps (for safety).

## Sample programs:

`>>55+55++M4$;` prints 4 exactly once (two IPs are created but cannot pass the `M` command until they merge and have 20 mana: the value on their stack

`>>55+55++4$;` prints 4 twice (both IPs reach the `4` and print it)

`>>55+55++:M$;` prints 20 once (again the two IPs need to merge before being able to pass the `M` command. Before executing `M` the combined IP will have a stack of [20,20] due to the `:` duplicate command, so after `M` it prints the single value left on the stack)

This program:

	 v$
	>31+$;
	 2^
	 +
	 $
	 ;;

Has the output `1,5,4`: the lower right IP reaches its `$` first, while the top IP is created first (and so its `$` is processed in the same tick as, but before, the leftmost IP's `$`).

`>4Qtd$;` prints the distance between the self-object and one other GameObject within radius 4 (in the sample scene this is `3.162278` and an empty stack)

## TODO:

 - Strings and Character literals, as well as string concatenation
 - Various other Unity GameObject or Physics tasks (this is primary for using the language as enchantments on RPG gear: event entry points, object instantiation, IFF, Raycasting, etc)
 - Stacks of Stacks? (e.g. ><>'s `[` and `]` operators)
 - Register(s) (e.g. ><>'s `&` operator)
 - Jump instruction? (e.g. ><>'s `.` operator)
 - Forking (a command that when executed spawns a new IP)
 - Error handling (prevent crashes, terminate execution when running infinitely, etc)