function [outIndex, revOutIndex] = findOrientTransIndex(refFilepath, srcFilepath)

    % check if the input files exist
    if exist(refFilepath, 'file') == 0
        fprintf(2, ['Error: invalid filepath to reference volume (', refFilepath, ')\n']);
        return;
    end
    if exist(srcFilepath, 'file') == 0
        fprintf(2, ['Error: invalid filepath to source volume (', srcFilepath, ')\n']);
        return;
    end
    
    % read the file
    refVolume = spm_vol(refFilepath);
    srcVolume = spm_vol(srcFilepath);
    
    reference = spm_read_vols(refVolume);
    source = spm_read_vols(srcVolume);

    % try all difference re-orientation transformations of the source data on the reference image
    % and pick the one that gives the smallest SSQ difference with the reference
    smallestSSQ = NaN;
    smallestOrientTransIndex = -1;
    for i= 0:37
        %todo: check the dimensions before re-orient, so we can dismiss
        %      re-orientations before doing them
        resMatrix = reorient3D(source, i);
        if (isequal(size(reference), size(resMatrix)))
            ssq = sumsqr(reference - resMatrix);
            if (isnan(smallestSSQ) || ssq < smallestSSQ)
               smallestSSQ = ssq;
               smallestOrientTransIndex = i;
            end
        end
    end
	
	%% TODO: this should very much be derived from the original orientation, however this is quick and dirty
	% try all difference re-orientation transformations of the reference data on the source image
    % and pick the one that gives the smallest SSQ difference with the reference
    smallestRevSSQ = NaN;
    smallestRevOrientTransIndex = -1;
    for i= 0:37
        %todo: check the dimensions before re-orient, so we can dismiss
        %      re-orientations before doing them
        resMatrix = reorient3D(reference, i);
        if (isequal(size(source), size(resMatrix)))
            ssq = sumsqr(source - resMatrix);
            if (isnan(smallestRevSSQ) || ssq < smallestRevSSQ)
               smallestRevSSQ = ssq;
               smallestRevOrientTransIndex = i;
            end
        end
    end
    
	
    % return the orientation transformation index that gives the best match
    outIndex = smallestOrientTransIndex;
    revOutIndex = smallestRevOrientTransIndex;
	
end