function [group, distance, ties] = multisvmclassify( multisvm, features, varargin )
	
	% Get the type from the model. (The function multisvmtrain stores this in the structure.)
	type = multisvm{1}.type;
	
	group = NaN( size(features,1), 1 );
	
	
	switch type
		case '1vsall'
			for f = 1 : size(features,1)	
				% Use the classifiers to classify. :)
				distance = cell2mat(cellfun( @(m) -svmDistanceToBoundary(m, features(f,:)), multisvm, 'Un', false ));
				ties = NaN;

				[~,highest] = max( distance );

				group(f) = setdiff(unique(multisvm{highest}.GroupNames),0);
			end
			
			
		case '1vs1'
			[group, ~, similarFrequency] = mode( cell2mat(cellfun( @(m) svmclassify(m, features, varargin{:}), multisvm, 'Un', false )), 2 );
			distance = [];
			ties = nnz( cellfun(@length,similarFrequency) > 1 );
	end
	
end