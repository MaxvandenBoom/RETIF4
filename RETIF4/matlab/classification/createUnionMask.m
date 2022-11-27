%   Create one mask based on a union of the voxels from seperate t-maps
%
%   outMask = createUnionMask(tmaps, cutoff, enableNegative, usePercentage) returns
%   a mask based on the lowest and/or highest voxels from t-maps.
% 
% 
%   tmaps               = a cell array with the 3D data of one or more t-maps
%   cutoff              = the absolute number of voxels or percentage (between 0 and 100) of voxels per t-map to be used
%   enableNegative      = 0 - only take positive voxels, 1 - take positive and negative voxels
%                         (note: when including negative voxels, be aware that the cutoff will be 
%                          applied at both the lower and upper end of the range, so the amount of 
%                          voxels will double in comparison to positive only)
%   usePercentage       = (optional) indicate whether the cutoff is an absolute number of
%                         voxels (0, default), or a percentage (1)
%
function outMask = createUnionMask(tmaps, cutoff, enableNegative, usePercentage)
	
    % optional input arguments
    if ~exist('enableNegative', 'var') || isempty(enableNegative) || enableNegative ~= 1
        enableNegative = 0;
    end
    if ~exist('usePercentage', 'var') || isempty(usePercentage) || usePercentage ~= 1
        usePercentage = 0;
    end
    
    % check cutoff input
    if usePercentage
        if cutoff <= 0
            fprintf(2, 'Error: union mask cutoff percentage should be above 0\n');
            return;
        end
        if cutoff > 100
            fprintf(2, 'Error: union mask cutoff percentage should be below 100\n');
            return;
        end
    else
        if cutoff < 1
            fprintf(2, 'Error: union mask number of voxels should be above 0\n');
            return;
        end
    end
    
	% select the higest ... voxels from each t-map and create a mask out of it
	masks = {};
	outMask = zeros(size(tmaps{1}));
	for i = 1:length(tmaps)
        
        masks{i} = tmaps{i};
        
        [maskvalues] = sort(tmaps{i}(:), 'descend');
        maskvalues(isnan(maskvalues)) = [];
        maskvalues(maskvalues <= 0) = [];
        
        % determine the positive cutoff value
        if usePercentage
            posCutVal = round(cutoff / 100 * length(maskvalues));
            posCutVal = maskvalues(min(posCutVal, length(maskvalues)));
        else
            posCutVal = maskvalues(min(cutoff, length(maskvalues)));
        end
        
        % set the positive values above the cutoff point to 1
        masks{i}(tmaps{i} >= posCutVal) = 1;
        
        % check if we also take deactivating voxels
        if enableNegative == 1
            
            [maskvalues] = sort(tmaps{i}(:), 'ascend');
            maskvalues(isnan(maskvalues)) = [];
            maskvalues(maskvalues >= 0) = [];
            
            % determine the negative cutoff value
            if usePercentage
                negCutVal = round(cutoff / 100 * length(maskvalues));
                negCutVal = maskvalues(min(negCutVal, length(maskvalues)));
            else
                negCutVal = maskvalues(min(cutoff, length(maskvalues)));
            end
            
            % set the negative values below the cutoff point to 1
            masks{i}(tmaps{i} <= negCutVal) = 1;
            
            % set the values which are between the negative and
            % positive cutoff point to 0
            masks{i}(tmaps{i} > negCutVal & tmaps{i} < posCutVal) = 0;
            
        else
            
            % set the values below the positive cutoff point to 0
            masks{i}(tmaps{i} < posCutVal) = 0;

        end
       
        % union in one single mask
        outMask(masks{i} == 1) = 1; 
        
    end
	
end