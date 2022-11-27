function [signalPeakIndices, signalPeakIndicesConditions, signal, signalMean] = cls_getFeaturePeaks(mask, volumes, peakIndices, indicesConditions)
    
    searchScansBeforePeak = 2;
    searchScansAfterPeak = 3;
    %discardTrialsBelow = 0.5;
    discardTrialsBelow = 100;
    bad_trials = [];
    skipStartVolumes = 5;

    %
    % masking
    %

    % extract the signal using the mask from the volumes
    logicalMask = logical(mask);
    signal = [];
    for i = 1:size(volumes, 4)
        volume = squeeze(volumes(:, :, :, i));
        signal(:, i) = volume(logicalMask);
    end
    %%%signal = volumes(mask, :)';


    %
    % normalization / detrending
    %


    % normalize / detrend the signal
    signal = cls_normalizeAndDetrendVolumeData(signal);




    % calculate the signal mean
    signalMean = mean(signal);

    

    signalPeakIndices = [];
    signalPeakIndicesConditions = [];
    signalPeakValues = [];
    for i = 1:length(peakIndices)
        scanIndex = peakIndices(i);

        % for the imagined the hrf is not so precize, so try to find the peak 4 scans
        % afterward and some scans forward, take the highest value
        highestValue = nan;
        highestValueIndex = nan;
        for j = scanIndex - searchScansBeforePeak:scanIndex + searchScansAfterPeak
           if isnan(highestValue) || (j < length(signalMean) && signalMean(j) > highestValue)
               highestValue = signalMean(j);
               highestValueIndex = j;
           end
        end
        scanIndex = highestValueIndex;

        % if the peak signal value is not higher than a given value then dismiss the trial
        %if highestValue < 102
        %if ~isnan(discardTrialsBelow) && highestValue < discardTrialsBelow
        %    continue;
        %end

        %{
        % dismiss the bad trials
        skiploop = 0;
        for j = 1:length(bad_trials)
            if bad_trials(j) == i

                % remove from bad trials list
                bad_trials(bad_trials == bad_trials(j)) = [];

                % skip further processing after this point (also in the parent
                % loop through skiploop)
                skiploop = 1;
                break;
            end
        end
        if skiploop == 1
            continue;
        end
        %}

        % add to signal peak index and condition to the list 
        signalPeakIndices(size(signalPeakIndices, 2) + 1) = highestValueIndex;
        signalPeakIndicesConditions(size(signalPeakIndicesConditions, 2) + 1) = indicesConditions(i);

        % add the values of the signal peak to the list
        signalPeakValues(:, size(signalPeakValues, 2) + 1) = signal(:, scanIndex);

    end
    clear highestValue highestValueIndex skiploop scanIndex i j;



end