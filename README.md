This is a very simple restaurant simulation (everything is in hungarian, sorry) from Q2 2017. It is based on a sample test in my university.

The goal was to practise threading in C# while having some fun.
Basically there are tables in the restaurant where new guests can sit. The restaurant has a couple servers (no, not computer servers) whose job is to give food to the guest (like appetizer, main dish, soup, etc.) and have them pay when they have finished eating. They are simulated by separate threads.
Every server has a 'cooldown' while they walk over to the next guest who is waiting for something, then enter cooldown again. And so on.

There is no goal/objective, it's just fun seeing how guests are handled.

Also I'm sorry, but this project is not documented as it was never really meant to be used for anything (except for me passing the test and the whole course :) )