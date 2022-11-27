function [valleyIndices, peakIndices, indicesConditions] = cls_getFeatureScansFromHrf(conditions, hrf, skipStartVolumes)
    
    % find the locations of peaks in the HRF model
    [~, peakIndices] = findpeaks(hrf);
    
    % check if there at least one peak
    if length(peakIndices) == 0
        fprintf(2, ['Error: No peaks were found, check hrf\n']);
        return;
    end
    
    % find the locations of valleys in the HRF model
    [~, valleyIndices] = findpeaks(max(hrf) - hrf);
    
    % remove any valley that comes after the last peak
    valleyIndices(valleyIndices > peakIndices(end)) = [];
    
    % find the lowest point before the first peak
    % and add the lowest point before the first peak as the first valley
    lowestPointIndex = -1;
    lowestPointValue = 0;
    for i = peakIndices(1):-1:1
        if lowestPointIndex == -1 || hrf(i) < lowestPointValue
            lowestPointIndex = i;
            lowestPointValue = hrf(i);
        end
    end
    valleyIndices = horzcat(lowestPointIndex, valleyIndices);
    clear lowestPointIndex lowestPointValue;

    % check if the number of valleys and peaks match
    if length(peakIndices) ~= length(valleyIndices)
        disp(['Number of peaks: ', num2str(length(peakIndices))]);
        disp(['Number of valleys: ', num2str(length(valleyIndices))]);
        fprintf(2, ['Error: Number of peaks and valleys in hrf do not match \n']);
        return;
    end
    
    % check if the number of conditions and the number of hrf peaks match
    if length(peakIndices) ~= length(conditions)
        disp(['Number of peaks: ', num2str(length(peakIndices))]);
        disp(['Number of conditions: ', num2str(length(conditions))]);
        fprintf(2, ['Error: Number of peaks in hrf and number of conditions do not match \n']);
        return;
    end
    
    % copy the conditions
    indicesConditions = conditions;
    
    % if peak or valleys are inside of the skipped volumes then discard the peak and condition
    skippedTrials = nnz(peakIndices <= skipStartVolumes | valleyIndices <= skipStartVolumes);
    for i = 1:skippedTrials
       peakIndices = peakIndices(2:end);
       valleyIndices = valleyIndices(2:end);
       indicesConditions = indicesConditions(2:end);
    end
    clear skippedTrials;

    %{
    % display hrf with peaks
    figure
    hrfDisplayPeak = nan(1, length(hrf));
    hrfDisplayPeak(peakIndices) = hrf(peakIndices);
    hrfDisplayValley = nan(1, length(hrf));
    hrfDisplayValley(valleyIndices) = hrf(valleyIndices);
    plot(hrf,'DisplayName','hrf', 'color', [1 .7 .4]);
    hold on;
    plot(hrfDisplayPeak,'o','DisplayName','Peaks');
    plot(hrfDisplayValley,'o','DisplayName','Valleys');
    hold off;
    %}

end