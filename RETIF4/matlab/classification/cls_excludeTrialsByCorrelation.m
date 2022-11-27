function [outSignalPeakIndices, outSignalPeakIndicesConditions] = cls_excludeTrialsByCorrelation(signal, signalPeakIndices, signalPeakIndicesConditions)

    minCorrelationValue = 0.4;

    % create cell array to store the excluded trials, previous excluded trials and previous correlation means in
    exclTrialIndices = {};
    prevExclTrialIndices = {};
    prevCorrelationsMeans = [];
    optimumExclusionReached = [];
    for i=1:max(signalPeakIndicesConditions) + 1
        exclTrialIndices{i} = [];
        prevExclTrialIndices{i} = [];
        prevCorrelationsMeans(i) = -999;
        optimumExclusionReached(i) = 0;
    end

    % iterate
    for iter = 1:1000
        
        % break the loop if for all symbols the optimum was reached
        if min(optimumExclusionReached) == 1
            break;
        end
        
        % create cell array to store the correlations, trial index values in
        correlations = {};
        correlationsTrialIndex = {};
        for i=1:max(signalPeakIndicesConditions) + 1
            correlations{i} = [];
            correlationsTrialIndex{i} = [];
        end

        % loop through all the trials
        for i = 1:size(signalPeakIndices, 2)

            % if the trial is excluded, then skip it
            if ismember(i, [exclTrialIndices{:}]) == 1
               continue; 
            end
            
            % copy the trials
            trainingTrials =  signal(:,signalPeakIndices);
            trainingTrialsConditions = signalPeakIndicesConditions;

            % retrieve the test trial
            testTrial = trainingTrials(:, i);
            testTrialCondition = signalPeakIndicesConditions(i);
            
            % remove the excluded trials and test trial
            trainingTrials(:, [i, [exclTrialIndices{:}]]) = [];
            trainingTrialsConditions(:, [i, [exclTrialIndices{:}]]) = [];
            
            % create the relevant template (the template for the symbol in the current trial)
            template = trainingTrials(:, (trainingTrialsConditions == signalPeakIndicesConditions(i)));
            template = mean(template, 2);

            % correlate the trial and template
            resCorr = corr(testTrial, template);
            if (resCorr < 0)
                resCorr = resCorr * -1;
            end

            % store the correlation value and the trial index
            correlations{signalPeakIndicesConditions(i) + 1} = vertcat(correlations{signalPeakIndicesConditions(i) + 1}, resCorr);
            correlationsTrialIndex{signalPeakIndicesConditions(i) + 1} = vertcat(correlationsTrialIndex{signalPeakIndicesConditions(i) + 1}, i);
            
        end
        
        disp('----');
        
        % loop through the symbols
        for i = 1:length(correlations)
            
            % check whether for this symbol the exclusion optimum was reached, skip the symbol if this is the case
            if optimumExclusionReached(i) == 1
                continue;
            end
            
            % calculate the mean correlation for the symbol
            correlationMean = mean(correlations{i});
            
            % message
            disp(['Symbol ', num2str(i - 1), ' - prev r = ', num2str(prevCorrelationsMeans(i)), '; current r ', num2str(correlationMean)]);
            
            
            if min(correlations{i}) > minCorrelationValue
                % all correlation higher than critical value
                
                % update the prev trial variables (since this criteria is
                % based on the current correlation values, not a comparison between this and the previous)
                prevExclTrialIndices{i} = exclTrialIndices{i};
                prevCorrelationsMeans(i) = correlationMean;
                
                % stop excluding
                optimumExclusionReached(i) = 1;

                % message
                disp(['Symbol ', num2str(i - 1), ' - optimum reached by min corration. mean r:', num2str(prevCorrelationsMeans(i)), '; excluded # trials: ', num2str(length(prevExclTrialIndices{i})), '; trials left: ', num2str(length(correlations{i}))]);
                
                % do not process any furter
                continue;
                
            end
            
            % check for every symbol if the exclusion of the trial led to a
            % better mean correlation (in comparison to the last run where the
            % trial was not removed)
            if correlationMean > prevCorrelationsMeans(i)
                % better mean, keep trial excluded and continue

                % update the prev trial variables
                prevExclTrialIndices{i} = exclTrialIndices{i};
                prevCorrelationsMeans(i) = correlationMean;

                % find the lowest correlating trial
                [~, lowestIndex] = min(correlations{i});

                % retrieve the actual index in the full inputted trial list
                lowestTrialIndex = correlationsTrialIndex{i}(lowestIndex);

                % message
                disp(['Symbol ', num2str(i - 1), ' - trying without trial ', num2str(lowestTrialIndex), ' (r: ', num2str(correlations{i}(lowestIndex)), ')']);

                % add the trial to the symbol's exclusion list
                exclTrialIndices{i} = [exclTrialIndices{i}, lowestTrialIndex];
            else
                % not better, stop excluding for this symbol

                % stop excluding
                optimumExclusionReached(i) = 1;

                % message
                disp(['Symbol ', num2str(i - 1), ' - optimum reached by mean. mean r:', num2str(prevCorrelationsMeans(i)), '; excluded # trials: ', num2str(length(prevExclTrialIndices{i})), '; trials left: ', num2str(length(correlations{i}) + 1)]);
                
            end
            

            
        end
        
    end
    
    %signalPeakIndices([prevExclTrialIndices{:}])
    
    outSignalPeakIndices = signalPeakIndices;
    outSignalPeakIndices([prevExclTrialIndices{:}]) = [];
    
    outSignalPeakIndicesConditions = signalPeakIndicesConditions;
    outSignalPeakIndicesConditions([prevExclTrialIndices{:}]) = [];
    
end

