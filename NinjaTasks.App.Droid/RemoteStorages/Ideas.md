## Change tracking of ContentProviders

Some ContentProviders might provider their own change tracking. To be not dependent on possibly buggy
implementations or limitations, though, these do not have to be used. Instead, to support change 
tracking, we merge the whole remote database to special tables in our database, and observe the 
changes in the usual way. 

Special care has to be taken with sort positions - if supported -, which is interdependent between 
different items in a list. We want to keep the number of updated items as little as possible, 
so a smart mapping is needed. Nothing has been implemented yet, though the mapping should probably be
based on  [cycle sort](http://www.geeksforgeeks.org/which-sorting-algorithm-makes-minimum-number-of-writes/).