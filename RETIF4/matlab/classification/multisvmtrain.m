function multisvm = multisvmtrain( features, groups, type, varargin )
	
	% Check the type. This can be one-versus-all ('1vsall') or one-versus-one ('1vs1'). In a '1vsall' approach, there are as many
	% classifiers created as there are classes. Each classifier makes a distinction between one class and all the others. In the
	% '1vs1' approach, there are k over n classifiers created. Each classifier makes a distinction between one class and one other
	% class.
	if nargin < 3, type = '1vs1'; end
	
	
	% Extract the possible labels.
	labels = unique(groups);
	
	switch lower(type)
		case '1vsall'
			% Create as many classifiers as there are labels. This is for 1-vs-all classification.
			multisvm = cell(1,length(labels));


			for group = 1 : length(labels)
				% Train the classifier for the current class 'group' against all other points that do not belong to the 'group'. (This is
				% the 'one-versus-others' approach.
				multisvm{group} = svmtrain( features, groups .* (groups == group), varargin{:} );
				
				
				% Add the multisvm type ('1vs1' or '1vsall'). This is used by the function multisvmclassify.
				multisvm{group}.type = type;


				% Draw a figure if necessary.
				if ~isempty( multisvm{group}.FigureHandles ) && group < length(labels)
					figure;
				end
			end
			
			
		case '1vs1'
			% Get all possible combinations of binary classifiers using the presented groups.
			allCombinations = combnk( labels, 2 );


			% Empty classifier array.
			multisvm = cell(1,length(allCombinations));


			% Train the classifiers.
			for combination = 1 : length(allCombinations)
				% Select the two groups (0 and 1) for the current classifier to train on.
				groupsForThisCombination = NaN( size(groups) );
				groupsForThisCombination( groups == allCombinations(combination,1) ) = allCombinations(combination,1);
				groupsForThisCombination( groups == allCombinations(combination,2) ) = allCombinations(combination,2);


				% Train the classifier on the two selected groups.
				multisvm{combination} = svmtrain( features, groupsForThisCombination, varargin{:} );


				% Add the multisvm type ('1vs1' or '1vsall'). This is used by the function multisvmclassify.
				multisvm{combination}.type = type;


				% Open a figure if necessary.
				if ~isempty( multisvm{combination}.FigureHandles ) && combination < length(labels)
					figure;
				end
			end
			
			
		otherwise
			error('The ''type'' should be ''1vs1'' or ''1vsall')
	end
end