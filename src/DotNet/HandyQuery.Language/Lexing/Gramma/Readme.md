# Query examples

## Covered by current gramma:

	FirstName = Damian
	FirstName is empty
	FirstName is not empty
	FirstName = firstNameFromUser("dkaminski", true)
	FirstName = firstNameFromUser(currentUser(), true)
	FirstName in("Damian", "Kamil")
	FirstName in("Damian", "Kamil", firstNameFromUser(currentUser()))
	FirstName = Damian OR FirstName = Kamil
	(FirstName = Damian OR FirstName = Kamil) AND LastName = Kaminski

## Maybe someday:

	FirstName = currentUser().firstName
	Employee.FirstName = "Damian"
	equals with case ignore?
	`ORDER BY` at the end of query

# Language gramma structure output (pseudocode):

```
// $Value = Literal|$FunctionInvokation
// $Filter = ?GroupOpen $FilterWithCompareOp|$FilterWithStatement ?GroupClose
// $FilterWithCompareOp = ColumnName CompareOperator $Value
// $FilterWithStatement = ColumnName Statement

var parts = {
	$Value: {
		name: "$Value",
		type: Part,
		body: [
			{ 
				name: null, 
				type: OrCondition, 
				operands: [
					{ name: "Literal", type: TokenizerUsage, optional: false, impl: tokenizers.Literal },
					{ name: "$FunctionInvokation", type: PartUsage, optional: false, impl: parts.$FunctionInvokation },
				]
			}
		]
	},
	$Filter: {
		name: "$Filter",
		type: Part,
		body: [
			{ name: "GroupOpen", type: TokenizerUsage, optional: true, impl: tokenizers.GroupOpen },
			{ 
				name: null, 
				type: OrCondition, 
				operands: [
					{ name: "$FilterWithCompareOp", type: PartUsage, optional: false, impl: parts.$FilterWithCompareOp },
					{ name: "$FilterWithStatement", type: PartUsage, optional: false, impl: parts.$FilterWithStatement }
				]
			},
			{ name: "GroupClose", type: TokenizerUsage, optional: true, impl: tokenizers.GroupClose },
		]
	},
	$FilterWithCompareOp: {
		name: "$FilterWithCompareOp",
		type: Part,
		body: [
			{ name: "ColumnName", type: TokenizerUsage, optional: false, impl: tokenizers.ColumnName },
			{ name: "CompareOperator", type: TokenizerUsage, optional: false, impl: tokenizers.CompareOperator },
			{ name: "$Value", type: PartUsage, optional: false, impl: parts.$Value }
		]
	}
	$FilterWithStatement: {
		name: "$FilterWithStatement",
		type: Part,
		body: [
			{ name: "ColumnName", type: TokenizerUsage, optional: false, impl: tokenizers.ColumnName },
			{ name: "Statement", type: TokenizerUsage, optional: false, impl: tokenizers.Statement }
		]
	}
}

var tokenizers = {
	Literal: {
		...
	},
	ColumnName: {
		...
	},
	Statement: {
		...
	},
	...
}
```