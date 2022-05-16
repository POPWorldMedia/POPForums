/// <reference path="../State/TopicState.ts" />

declare namespace Popper {
    function createPopper(el: Element, popper:HTMLElement, options:any): void;
}
declare var topicState: TopicState;
declare namespace PopForums {
    function loadReply(topicID: number, postID: number, replyID: number, setupMorePosts: boolean): void;
    export interface TopicState{
        replyLoaded: boolean;
    }
}

class QuoteButton extends HTMLElement {
    constructor() {
        super();
    }

    get name(): string {
        return this.getAttribute("name");
    }
    get containerid(): string {
        return this.getAttribute("containerid");
    }
    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }
    get buttontext(): string {
        return this.getAttribute("buttontext");
    }
    get tip(): string {
        return this.getAttribute("tip");
    }
    get postID(): string {
        return this.getAttribute("postid");
    }

    connectedCallback() {
        let targetText = document.getElementById(this.containerid);
        this.innerHTML = QuoteButton.template;
        let button = this.querySelector("input");
        let tip = this.querySelector('#tooltip') as HTMLElement;
        tip.innerHTML = this.tip + `<div id="arrow" data-popper-arrow></div>`;
        ["mousedown","touchstart"].forEach((e:string) => targetText.addEventListener(e, () => tip.removeAttribute("data-show")));
        button.value = this.buttontext;
        let classes = this.buttonclass;
        if (classes?.length > 0)
            classes.split(" ").forEach((c) => button.classList.add(c));
        this.onclick = (e: MouseEvent) => {
            let selection = document.getSelection();
            if (selection.rangeCount === 0 || selection.getRangeAt(0).toString().length === 0) {
                // prompt to select
                const popperInstance = Popper.createPopper(button, tip, {
                    modifiers: [
                      {
                        name: 'offset',
                        options: {offset: [0, 8]}
                      }
                    ],
                  });
                tip.setAttribute('data-show', '');
                selection.removeAllRanges();
                return;
            }
            let range = selection.getRangeAt(0);
            let fragment = range.cloneContents();
            let div = document.createElement("div");
            div.appendChild(fragment);
            // is selection in the container?
            let ancestor = range.commonAncestorContainer;
            while (ancestor['id'] !== this.containerid && ancestor.parentElement !== null) {
                ancestor = ancestor.parentElement;
            }
            let isInText = ancestor['id'] === this.containerid;
            // if not, is it partially in the container?
            if (!isInText) {
                let container = div.querySelector("#" + this.containerid);
                if (container !== null && container !== undefined) {
                    // it's partially in the container, so just get that part
                    div.innerHTML = container.innerHTML;
                    isInText = true;
                }
            }
            selection.removeAllRanges();
            if (isInText) {
                // activate or add to quote
                let result: string;
                if (topicState.isPlainText)
                    result = `[quote][i]${this.name}:[/i]\r\n ${div.innerText}[/quote]`;
                else
                    result = `<blockquote><p><i>${this.name}:</i></p>${div.innerHTML}</blockquote><p></p>`;
                topicState.nextQuote = result;
                if (!topicState.isReplyLoaded)
                    PopForums.loadReply(topicState.topicID, null, Number(this.postID), true);
            }
        };
    }

    static template: string = `<style>
    #tooltip {
      background: #333;
      color: white;
      font-weight: bold;
      padding: 4px 8px;
      font-size: 13px;
      border-radius: 4px;
      display: none;
    }

    #tooltip[data-show] {
      display: block;
    }

    #arrow,
    #arrow::before {
      position: absolute;
      width: 8px;
      height: 8px;
      background: inherit;
    }

    #arrow {
      visibility: hidden;
    }

    #arrow::before {
      visibility: visible;
      content: '';
      transform: rotate(45deg);
    }

    #tooltip[data-popper-placement^='top'] > #arrow {
      bottom: -4px;
    }

    #tooltip[data-popper-placement^='bottom'] > #arrow {
      top: -4px;
    }

    #tooltip[data-popper-placement^='left'] > #arrow {
      right: -4px;
    }

    #tooltip[data-popper-placement^='right'] > #arrow {
      left: -4px;
    }
    </style>
    <div id="tooltip" role="tooltip"></div>
    <input type="button" />`;
}

customElements.define('pf-quotebutton', QuoteButton);