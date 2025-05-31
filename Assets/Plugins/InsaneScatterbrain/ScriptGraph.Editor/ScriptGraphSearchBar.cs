using System.Collections.Generic;
using System.Timers;
using InsaneScatterbrain.Threading;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class ScriptGraphSearchBar : Toolbar
    {
        private readonly Label currentResultLabel;
        private readonly Label resultSeparatorLabel;
        private readonly Label numTotalResultsLabel;
        private readonly Timer searchTimer;
        private readonly ToolbarSearchField searchField;
        
        private readonly List<INode> searchResults = new List<INode>();
        private int currentSearchResultIndex;

        private readonly MainThreadCommand updateSearchResultMainThreadCommand;

        private readonly ScriptGraphView graphView;
        
        public ScriptGraphSearchBar(ScriptGraphView graphView)
        {
            this.graphView = graphView;
            
            // Start closed
            style.display = DisplayStyle.None;
            style.unityTextAlign = TextAnchor.MiddleCenter;
            style.flexDirection = FlexDirection.Row;
            
            updateSearchResultMainThreadCommand = new MainThreadCommand(UpdateSearchResult);

            // Initialize the timer
            searchTimer = new Timer(300);
            searchTimer.AutoReset = false;
            searchTimer.Elapsed += (sender, e) => ExecuteSearch();

            searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(evt =>
            {
                // (Re)set the timer, so that the search is only executed after the user has stopped typing.
                searchTimer.Stop();
                searchTimer.Start();
            });
            
            // On enter, execute select the next search result.
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    NextResult();
                }
                
                if (evt.keyCode == KeyCode.Escape)
                {
                    Hide();
                }
            });
            
            Add(searchField);
            
            var previousResultButton = new ToolbarButton(PreviousResult)
            {
                text = "\u2190",
                tooltip = "Previous result",
                style =
                {
                    borderRightWidth = 0
                }
            };

            var nextResultButton = new ToolbarButton(NextResult)
            {
                text = "→",
                tooltip = "Next result"
            };

            currentResultLabel = new Label
            {
                text = "0",
                style =
                {
                    marginTop = 2,
                    marginLeft = 5
                }
            };

            resultSeparatorLabel = new Label
            {
                text = "/",
                style =
                {
                    marginTop = 2
                }
            };

            numTotalResultsLabel = new Label
            {
                text = "0",
                style =
                {
                    marginTop = 2,
                    marginRight = 5
                }
            };
            
            var closeButton = new ToolbarButton(Hide)
            {
                text = "\u2715",
                tooltip = "Close search bar (ESC)",
                style =
                {
                    left = 0,
                    borderRightWidth = 0
                }
            };

            var closeButtonWrapper = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd
                }
            };
            closeButtonWrapper.Add(closeButton);
            
            Add(currentResultLabel);
            Add(resultSeparatorLabel);
            Add(numTotalResultsLabel);
            Add(previousResultButton);
            Add(nextResultButton);
            Add(closeButtonWrapper);
        }
        
        private void ExecuteSearch()
        {
            graphView.ClearHighlights();
            var searchTerm = searchField.value.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                currentSearchResultIndex = 0;
                searchResults.Clear();
                MainThread.Execute(updateSearchResultMainThreadCommand);
                return;
            }

            searchResults.Clear();
            
            foreach (var node in graphView.Graph.Nodes)
            {
                var view = graphView.GetView(node);
                
                var titleContainsSearchTerm = view.title.ToLower().Contains(searchTerm);
                var noteContainsSearchTerm = node.Note != null && node.Note.ToLower().Contains(searchTerm);
                
                if (!titleContainsSearchTerm && !noteContainsSearchTerm)
                {
                    continue;
                }
                
                searchResults.Add(node);
                graphView.HighlightSearchedNode(node);
            }

            foreach (var inputNode in graphView.Graph.InputNodes)
            {
                if (searchResults.Contains(inputNode))
                {
                    continue;
                }
                
                var id = inputNode.InputParameterId;
                var parameterName = graphView.Graph.InputParameters.GetName(id);
                if (!parameterName.ToLower().Contains(searchTerm))
                {
                    continue;
                }
                
                searchResults.Add(inputNode);
                graphView.HighlightSearchedNode(inputNode);
            }
            
            foreach (var outputNode in graphView.Graph.OutputNodes)
            {
                if (searchResults.Contains(outputNode))
                {
                    continue;
                }
                
                var id = outputNode.OutputParameterId;
                var parameterName = graphView.Graph.OutputParameters.GetName(id);
                if (!parameterName.ToLower().Contains(searchTerm))
                {
                    continue;
                }
                
                searchResults.Add(outputNode);
                graphView.HighlightSearchedNode(outputNode);
            }
            
            foreach (var referenceNode in graphView.Graph.ReferenceNodes)
            {
                var view = graphView.GetReferenceNodeView(referenceNode);
                if (!view.title.ToLower().Contains(searchTerm))
                {
                    continue;
                }
                
                searchResults.Add(referenceNode);
                graphView.HighlightSearchedNode(referenceNode);
            }

            foreach (var group in graphView.Graph.GroupNodes)
            {
                var view = graphView.GetGroupView(group);
                if (!view.title.ToLower().Contains(searchTerm))
                {
                    continue;
                }
                
                searchResults.Add(group);
                graphView.HighlightSearchedNode(group);
            }
            
#if UNITY_2020_1_OR_NEWER
            foreach (var note in graphView.Graph.Notes)
            {
                var view = graphView.GetNoteView(note);
                if (!note.Title.ToLower().Contains(searchTerm) &&
                    !note.Contents.ToLower().Contains(searchTerm))
                {
                    continue;
                }
                
                searchResults.Add(note);
                graphView.HighlightSearchedNode(note);
            }
#endif
            
            // Sort the search results by their position in the graph.
            searchResults.Sort((a, b) =>
            {
                // Sort the nodes from top to bottom and left to right.
                var aPos = a.Position;
                var bPos = b.Position;
                
                if (aPos.y < bPos.y)
                {
                    return -1;
                }
                
                if (aPos.y > bPos.y)
                {
                    return 1;
                }
                
                if (aPos.x < bPos.x)
                {
                    return -1;
                }
                
                if (aPos.x > bPos.x)
                {
                    return 1;
                }
                
                return 0;
            });
            
            currentSearchResultIndex = 0;
            MainThread.Execute(updateSearchResultMainThreadCommand);
        }
        
        private void UpdateSearchResult()
        {
            numTotalResultsLabel.text = searchResults.Count.ToString();
            currentResultLabel.text = searchResults.Count > 0 ? (currentSearchResultIndex + 1).ToString() : "0";
            
            if (searchResults.Count == 0)
            {
                return;
            }
            
            graphView.FrameNode(searchResults[currentSearchResultIndex]);
        }

        private void SelectResult(int index)
        {
            if (searchResults.Count == 0) return;

            currentSearchResultIndex = index % searchResults.Count;
            UpdateSearchResult();
        }

        private void PreviousResult()
        {
            SelectResult(currentSearchResultIndex - 1 + searchResults.Count);
        }
        
        private void NextResult()
        {
            SelectResult(currentSearchResultIndex + 1);
        }
        
        public void Toggle()
        {
            if (style.display == DisplayStyle.None)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            style.display = DisplayStyle.Flex;
            searchField.value = string.Empty;
            searchField.Q("unity-text-input").Focus();
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
            searchField.value = string.Empty;
            graphView.ClearHighlights();
        }

        public void OnGUI()
        {
            var color = GUI.skin.label.normal.textColor;
            currentResultLabel.style.color = color;
            resultSeparatorLabel.style.color = color;
            numTotalResultsLabel.style.color = color;
            
            var e = Event.current;
            
            if (e.keyCode == KeyCode.Escape)
            {
                Hide();
            }
            
            if (e.keyCode == KeyCode.F && e.modifiers == EventModifiers.Control)
            {
                Show();
            }
        }
    }
}